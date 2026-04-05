namespace ContentUnderstanding.Api.Models;

public record AnalysisRequest(string FileName, string ContentType);

public record AnalyzeUrlRequest(string Url, string Scenario = "video");

public record TranscriptSegment(
    string Speaker,
    string Text,
    double StartTimeSeconds,
    double EndTimeSeconds);

public record AnalysisResponse(
    string Id,
    string Status,
    string FileName,
    string Scenario,
    DateTimeOffset AnalyzedAt,
    double? AnalysisDurationMs,
    IReadOnlyDictionary<string, FieldResult>? Fields,
    string? Summary = null,
    IReadOnlyList<TranscriptSegment>? Transcript = null);

public record FieldResult(string Name, object? Value, float? Confidence, string? Type = null);
