using ContentUnderstanding.Api.Services;

namespace ContentUnderstanding.Api.Endpoints;

public static class AnalyzeEndpoints
{
    public static void MapAnalyzeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/analyze")
            .WithTags("Analysis")
            .DisableAntiforgery();

        group.MapPost("/", async (IFormFile file, IContentAnalysisService service, string scenario = "invoice", CancellationToken ct = default) =>
        {
            if (file.Length == 0)
                return Results.BadRequest("No file provided.");

            if (file.Length > 10 * 1024 * 1024)
                return Results.BadRequest("File exceeds 10 MB limit.");

            await using var stream = file.OpenReadStream();
            var result = await service.AnalyzeAsync(stream, file.FileName, file.ContentType, scenario, ct);
            return Results.Ok(result);
        })
        .WithName("AnalyzeContent")
        .WithDescription("Upload a file for content analysis");

        group.MapGet("/{id}", async (string id, IContentAnalysisService service, CancellationToken ct) =>
        {
            var result = await service.GetResultAsync(id, ct);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetAnalysisResult")
        .WithDescription("Retrieve a previous analysis result");
    }
}
