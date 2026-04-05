using System.Collections.Concurrent;
using System.Diagnostics;
using Azure;
using Azure.AI.ContentUnderstanding;
using Azure.Identity;
using ContentUnderstanding.Api.Models;

namespace ContentUnderstanding.Api.Services;

public sealed class ContentAnalysisService : IContentAnalysisService
{
    private readonly ContentUnderstandingClient _client;
    private readonly IReadOnlyDictionary<string, string> _analyzers;
    private readonly ILogger<ContentAnalysisService> _logger;
    private readonly ConcurrentDictionary<string, AnalysisResponse> _resultCache = new();

    public ContentAnalysisService(IConfiguration configuration, ILogger<ContentAnalysisService> logger)
    {
        _logger = logger;

        var endpoint = configuration["AzureContentUnderstanding:Endpoint"]
            ?? throw new InvalidOperationException("AzureContentUnderstanding:Endpoint is not configured.");

        var analyzersSection = configuration.GetSection("AzureContentUnderstanding:Analyzers");
        var analyzers = analyzersSection.GetChildren()
            .ToDictionary(x => x.Key, x => x.Value ?? throw new InvalidOperationException($"Analyzer '{x.Key}' has no value."));

        if (analyzers.Count == 0)
            throw new InvalidOperationException("No analyzers configured in AzureContentUnderstanding:Analyzers.");

        _analyzers = analyzers;
        var options = new DefaultAzureCredentialOptions { 
            TenantId = "c37c6b03-6e61-405b-8224-98bebfcdbbeb" 
        };
        _client = new ContentUnderstandingClient(new Uri(endpoint), new DefaultAzureCredential(options));
    }

    public async Task<AnalysisResponse> AnalyzeAsync(Stream content, string fileName, string contentType, string scenario = "invoice", CancellationToken cancellationToken = default)
    {
        if (!_analyzers.TryGetValue(scenario, out var analyzerId))
            throw new ArgumentException($"Unknown analysis scenario: '{scenario}'. Available: {string.Join(", ", _analyzers.Keys)}");

        _logger.LogInformation("Analyzing file {FileName} ({ContentType}) with scenario {Scenario}",
            fileName, contentType, scenario);

        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, cancellationToken);
        var binaryData = BinaryData.FromBytes(ms.ToArray());

        var stopwatch = Stopwatch.StartNew();
        var operation = await _client.AnalyzeBinaryAsync(
            WaitUntil.Completed,
            analyzerId,
            binaryData,
            contentType: contentType,
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        _logger.LogInformation("Azure Content Understanding API call took {DurationMs:F0} ms for {FileName}",
            stopwatch.Elapsed.TotalMilliseconds, fileName);

        var result = operation.Value;
        var fields = ExtractFields(result);

        var response = new AnalysisResponse(
            Id: operation.Id,
            Status: "succeeded",
            FileName: fileName,
            Scenario: scenario,
            AnalyzedAt: result.CreatedAt ?? DateTimeOffset.UtcNow,
            AnalysisDurationMs: stopwatch.Elapsed.TotalMilliseconds,
            Fields: fields);

        _resultCache[operation.Id] = response;

        _logger.LogInformation("Analysis completed for {FileName}: {FieldCount} fields extracted",
            fileName, fields?.Count ?? 0);

        return response;
    }

    public async Task<AnalysisResponse> AnalyzeUrlAsync(string url, string scenario, CancellationToken cancellationToken = default)
    {
        if (!_analyzers.TryGetValue(scenario, out var analyzerId))
            throw new ArgumentException($"Unknown analysis scenario: '{scenario}'. Available: {string.Join(", ", _analyzers.Keys)}");

        _logger.LogInformation("Analyzing URL {Url} with scenario {Scenario}", url, scenario);

        var stopwatch = Stopwatch.StartNew();
        var operation = await _client.AnalyzeAsync(
            WaitUntil.Completed,
            analyzerId,
            inputs: new[] { new AnalysisInput { Uri = new Uri(url) } },
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        _logger.LogInformation("Azure Content Understanding API call took {DurationMs:F0} ms for URL {Url}",
            stopwatch.Elapsed.TotalMilliseconds, url);

        var result = operation.Value;
        var (summary, transcript) = ExtractVideoContent(result);

        var fileName = Uri.TryCreate(url, UriKind.Absolute, out var uri)
            ? System.IO.Path.GetFileName(uri.LocalPath)
            : url;

        var response = new AnalysisResponse(
            Id: operation.Id,
            Status: "succeeded",
            FileName: string.IsNullOrEmpty(fileName) ? url : fileName,
            Scenario: scenario,
            AnalyzedAt: result.CreatedAt ?? DateTimeOffset.UtcNow,
            AnalysisDurationMs: stopwatch.Elapsed.TotalMilliseconds,
            Fields: null,
            Summary: summary,
            Transcript: transcript);

        _resultCache[operation.Id] = response;

        _logger.LogInformation("URL analysis completed for {Url}: summary={HasSummary}, transcript={SegmentCount} segments",
            url, summary is not null, transcript?.Count ?? 0);

        return response;
    }

    public Task<AnalysisResponse?> GetResultAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving analysis result {Id}", id);
        _resultCache.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    private static (string? summary, IReadOnlyList<TranscriptSegment>? transcript) ExtractVideoContent(AnalysisResult result)
    {
        if (result.Contents is null or { Count: 0 })
            return (null, null);

        // Summary — from first AudioVisualContent item's Fields dictionary (capital S)
        string? summary = null;
        if (result.Contents[0] is AudioVisualContent firstAv
            && firstAv.Fields?.TryGetValue("Summary", out var summaryField) == true
            && summaryField is ContentStringField summaryStr)
        {
            summary = summaryStr.Value;
        }

        // Transcript — from TranscriptPhrases on each AudioVisualContent segment
        var segments = new List<TranscriptSegment>();
        foreach (var content in result.Contents)
        {
            if (content is not AudioVisualContent avContent)
                continue;

            if (avContent.TranscriptPhrases is null)
                continue;

            foreach (var phrase in avContent.TranscriptPhrases)
            {
                if (string.IsNullOrWhiteSpace(phrase.Text))
                    continue;

                string speaker = phrase.Speaker ?? "Unknown";
                double startSec = phrase.StartTime.TotalSeconds;
                double endSec = phrase.EndTime.TotalSeconds;
                segments.Add(new TranscriptSegment(speaker, phrase.Text, startSec, endSec));
            }
        }

        return (summary, segments.Count > 0 ? segments : null);
    }

    private static IReadOnlyDictionary<string, FieldResult>? ExtractFields(AnalysisResult result)
    {
        var content = result.Contents?.FirstOrDefault();
        if (content?.Fields is null || content.Fields.Count == 0)
            return null;

        var fields = new Dictionary<string, FieldResult>();
        foreach (var (name, field) in content.Fields)
        {
            fields[name] = ConvertField(name, field);
        }

        return fields;
    }

    private static FieldResult ConvertField(string name, ContentField field)
    {
        var (value, type) = field switch
        {
            ContentStringField f => ((object?)f.Value, "string"),
            ContentNumberField f => ((object?)f.Value, "number"),
            ContentIntegerField f => ((object?)f.Value, "integer"),
            ContentBooleanField f => ((object?)f.Value, "boolean"),
            ContentDateTimeOffsetField f => ((object?)f.Value, "date"),
            ContentTimeField f => ((object?)f.Value, "time"),
            ContentObjectField f => ((object?)ConvertObjectFields(f.Value), "object"),
            ContentArrayField f => ((object?)ConvertArrayItems(f.Value), "array"),
            _ => ((object?)field.Value?.ToString(), "unknown")
        };

        return new FieldResult(name, value, field.Confidence, type);
    }

    private static Dictionary<string, FieldResult>? ConvertObjectFields(IDictionary<string, ContentField>? fields)
    {
        if (fields is null or { Count: 0 })
            return null;

        return fields.ToDictionary(kvp => kvp.Key, kvp => ConvertField(kvp.Key, kvp.Value));
    }

    private static List<FieldResult>? ConvertArrayItems(IList<ContentField>? items)
    {
        if (items is null or { Count: 0 })
            return null;

        return items.Select((item, i) => ConvertField($"[{i}]", item)).ToList();
    }
}
