using System.Net;
using System.Net.Http.Json;
using ContentUnderstanding.Api.Models;
using NSubstitute;
using Xunit;

namespace ContentUnderstanding.Api.Tests;

public class AnalyzeUrlEndpointTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;
    private readonly ApiFixture _fixture;

    public AnalyzeUrlEndpointTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Post_AnalyzeUrl_WithValidUrl_ReturnsOkWithResponse()
    {
        // Arrange
        // NOTE: compiles once Neo adds TranscriptSegment and Summary/Transcript fields to AnalysisResponse
        var expectedResponse = new AnalysisResponse(
            Id: "url-abc-123",
            Status: "Succeeded",
            FileName: "https://example.com/video.mp4",
            Scenario: "video",
            AnalyzedAt: DateTimeOffset.UtcNow,
            AnalysisDurationMs: null,
            Fields: new Dictionary<string, FieldResult>
            {
                ["Title"] = new FieldResult("Title", "Sample Video", 0.95f, "string")
            },
            Summary: "This video contains an overview of Q1 results.",
            Transcript: new List<TranscriptSegment>
            {
                new TranscriptSegment("speaker1", "Good morning everyone.", 0.0, 2.4),
                new TranscriptSegment("speaker2", "Thanks for joining.", 2.8, 4.1)
            });

        _fixture.MockService
            .AnalyzeUrlAsync(
                Arg.Is("https://example.com/video.mp4"),
                Arg.Is("video"),
                Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var response = await _client.PostAsJsonAsync("/api/analyze/url",
            new { url = "https://example.com/video.mp4", scenario = "video" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AnalysisResponse>();
        Assert.NotNull(result);
        Assert.Equal("url-abc-123", result.Id);
        Assert.Equal("Succeeded", result.Status);
        Assert.Equal("video", result.Scenario);
        Assert.NotNull(result.Fields);
        Assert.True(result.Fields.ContainsKey("Title"));
        Assert.NotNull(result.Summary);
        Assert.NotEmpty(result.Summary);
        Assert.NotNull(result.Transcript);
        Assert.Equal(2, result.Transcript.Count);
    }

    [Fact]
    public async Task Post_AnalyzeUrl_WithEmptyUrl_ReturnsBadRequest()
    {
        // Arrange — URL is an empty string
        // Act
        var response = await _client.PostAsJsonAsync("/api/analyze/url",
            new { url = "", scenario = "video" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_AnalyzeUrl_WithMissingUrl_ReturnsBadRequest()
    {
        // Arrange — URL field is absent from body (null)
        // Act
        var response = await _client.PostAsJsonAsync("/api/analyze/url",
            new { url = (string?)null, scenario = "video" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_AnalyzeUrl_WithVideoScenario_ReturnsMatchingScenario()
    {
        // Arrange
        var expectedResponse = new AnalysisResponse(
            Id: "video-scenario-001",
            Status: "Succeeded",
            FileName: "https://example.com/clip.mp4",
            Scenario: "video",
            AnalyzedAt: DateTimeOffset.UtcNow,
            AnalysisDurationMs: null,
            Fields: new Dictionary<string, FieldResult>
            {
                ["Duration"] = new FieldResult("Duration", "00:02:30", 0.99f, "string")
            });

        _fixture.MockService
            .AnalyzeUrlAsync(
                Arg.Any<string>(),
                Arg.Is("video"),
                Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var response = await _client.PostAsJsonAsync("/api/analyze/url",
            new { url = "https://example.com/clip.mp4", scenario = "video" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AnalysisResponse>();
        Assert.NotNull(result);
        Assert.Equal("video", result.Scenario);
        Assert.Equal("video-scenario-001", result.Id);
    }

    [Fact]
    // NOTE: compiles once Neo adds TranscriptSegment and Summary/Transcript fields to AnalysisResponse
    public async Task Post_AnalyzeUrl_WithVideoScenario_ReturnsSummaryAndTranscript()
    {
        // Arrange
        var expectedResponse = new AnalysisResponse(
            Id: "video-transcript-001",
            Status: "Succeeded",
            FileName: "https://example.com/meeting.mp4",
            Scenario: "video",
            AnalyzedAt: DateTimeOffset.UtcNow,
            AnalysisDurationMs: 4200.0,
            Fields: null,
            Summary: "A recorded team meeting discussing project milestones.",
            Transcript: new List<TranscriptSegment>
            {
                new TranscriptSegment("speaker1", "Let's get started.", 0.0, 1.8),
                new TranscriptSegment("speaker2", "Thanks, I'll go first.", 2.1, 4.5),
                new TranscriptSegment("speaker1", "Great, please proceed.", 5.0, 7.3)
            });

        _fixture.MockService
            .AnalyzeUrlAsync(
                Arg.Any<string>(),
                Arg.Is("video"),
                Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var response = await _client.PostAsJsonAsync("/api/analyze/url",
            new { url = "https://example.com/meeting.mp4", scenario = "video" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AnalysisResponse>();
        Assert.NotNull(result);
        Assert.Equal("A recorded team meeting discussing project milestones.", result.Summary);
        Assert.NotNull(result.Transcript);
        Assert.Equal(3, result.Transcript.Count);
        Assert.Equal("speaker1", result.Transcript[0].Speaker);
        Assert.True(result.Transcript[1].StartTimeSeconds > 0);
    }

    [Fact]
    // NOTE: compiles once Neo adds TranscriptSegment and Summary/Transcript fields to AnalysisResponse
    public async Task Post_AnalyzeUrl_WithNoTranscript_ReturnsNullTranscript()
    {
        // Arrange
        var expectedResponse = new AnalysisResponse(
            Id: "no-transcript-001",
            Status: "Succeeded",
            FileName: "https://example.com/silent.mp4",
            Scenario: "video",
            AnalyzedAt: DateTimeOffset.UtcNow,
            AnalysisDurationMs: null,
            Fields: null,
            Summary: "Brief summary",
            Transcript: null);

        _fixture.MockService
            .AnalyzeUrlAsync(
                Arg.Any<string>(),
                Arg.Is("video"),
                Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var response = await _client.PostAsJsonAsync("/api/analyze/url",
            new { url = "https://example.com/silent.mp4", scenario = "video" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AnalysisResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Summary);
        Assert.Null(result.Transcript);
    }
}
