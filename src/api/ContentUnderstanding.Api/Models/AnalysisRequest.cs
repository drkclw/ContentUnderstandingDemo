namespace ContentUnderstanding.Api.Models;

public record AnalysisRequest(string FileName, string ContentType);

public record AnalysisResponse(
    string Id,
    string Status,
    string FileName,
    string Scenario,
    DateTimeOffset AnalyzedAt,
    IReadOnlyDictionary<string, FieldResult>? Fields);

public record FieldResult(string Name, object? Value, float? Confidence, string? Type = null);
