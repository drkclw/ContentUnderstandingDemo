using ContentUnderstanding.Api.Models;

namespace ContentUnderstanding.Api.Services;

public interface IContentAnalysisService
{
    Task<AnalysisResponse> AnalyzeAsync(Stream content, string fileName, string contentType, string scenario = "invoice", CancellationToken cancellationToken = default);
    Task<AnalysisResponse> AnalyzeUrlAsync(string url, string scenario, CancellationToken cancellationToken = default);
    Task<AnalysisResponse?> GetResultAsync(string id, CancellationToken cancellationToken = default);
}
