using System.Net;
using System.Net.Http.Json;
using ContentUnderstanding.Api.Models;
using NSubstitute;
using Xunit;

namespace ContentUnderstanding.Api.Tests;

public class AnalyzeEndpointTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;
    private readonly ApiFixture _fixture;

    public AnalyzeEndpointTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    private static MultipartFormDataContent CreateFileContent(byte[] fileBytes, string fileName = "test.pdf", string contentType = "application/pdf")
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);
        return content;
    }

    [Fact]
    public async Task Post_Analyze_WithValidFile_ReturnsOkWithResponse()
    {
        // Arrange
        var expectedResponse = new AnalysisResponse(
            Id: "abc-123",
            Status: "Succeeded",
            FileName: "test.pdf",
            Scenario: "invoice",
            AnalyzedAt: DateTimeOffset.UtcNow,
            AnalysisDurationMs: null,
            Fields: new Dictionary<string, FieldResult>
            {
                ["Title"] = new FieldResult("Title", "Sample Document", 0.95f, "string")
            });

        _fixture.MockService
            .AnalyzeAsync(Arg.Any<Stream>(), Arg.Is("test.pdf"), Arg.Is("application/pdf"), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        var formContent = CreateFileContent("Hello PDF content"u8.ToArray());

        // Act
        var response = await _client.PostAsync("/api/analyze", formContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AnalysisResponse>();
        Assert.NotNull(result);
        Assert.Equal("abc-123", result.Id);
        Assert.Equal("Succeeded", result.Status);
        Assert.Equal("test.pdf", result.FileName);
        Assert.NotNull(result.Fields);
        Assert.True(result.Fields.ContainsKey("Title"));
    }

    [Fact]
    public async Task Post_Analyze_WithEmptyFile_ReturnsBadRequest()
    {
        // Arrange — zero-length file
        var formContent = CreateFileContent([]);

        // Act
        var response = await _client.PostAsync("/api/analyze", formContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Analyze_WithOversizedFile_ReturnsBadRequest()
    {
        // Arrange — 11 MB file
        var oversized = new byte[11 * 1024 * 1024];
        var formContent = CreateFileContent(oversized);

        // Act
        var response = await _client.PostAsync("/api/analyze", formContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_AnalyzeById_WhenExists_ReturnsOk()
    {
        // Arrange
        var expectedResponse = new AnalysisResponse(
            Id: "existing-id",
            Status: "Succeeded",
            FileName: "report.docx",
            Scenario: "invoice",
            AnalyzedAt: DateTimeOffset.UtcNow,
            AnalysisDurationMs: null,
            Fields: null);

        _fixture.MockService
            .GetResultAsync("existing-id", Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var response = await _client.GetAsync("/api/analyze/existing-id");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AnalysisResponse>();
        Assert.NotNull(result);
        Assert.Equal("existing-id", result.Id);
    }

    [Fact]
    public async Task Post_Analyze_WithComplexFields_SerializesNestedTypesCorrectly()
    {
        // Arrange — response with object and array field types
        var expectedResponse = new AnalysisResponse(
            Id: "complex-123",
            Status: "Succeeded",
            FileName: "invoice.pdf",
            Scenario: "invoice",
            AnalyzedAt: DateTimeOffset.UtcNow,
            AnalysisDurationMs: null,
            Fields: new Dictionary<string, FieldResult>
            {
                ["VendorName"] = new FieldResult("VendorName", "Contoso Ltd", 0.98f, "string"),
                ["InvoiceTotal"] = new FieldResult("InvoiceTotal", 1250.00, 0.97f, "number"),
                ["IsVerified"] = new FieldResult("IsVerified", true, 0.99f, "boolean"),
                ["VendorAddress"] = new FieldResult("VendorAddress",
                    new Dictionary<string, FieldResult>
                    {
                        ["Street"] = new FieldResult("Street", "123 Main St", 0.90f, "string"),
                        ["City"] = new FieldResult("City", "Seattle", 0.92f, "string")
                    }, 0.91f, "object"),
                ["LineItems"] = new FieldResult("LineItems",
                    new List<FieldResult>
                    {
                        new FieldResult("Item1", "Widget A", 0.88f, "string"),
                        new FieldResult("Item2", "Widget B", 0.87f, "string")
                    }, 0.85f, "array")
            });

        _fixture.MockService
            .AnalyzeAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        var formContent = CreateFileContent("invoice content"u8.ToArray(), "invoice.pdf");

        // Act
        var response = await _client.PostAsync("/api/analyze", formContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("VendorName", json);
        Assert.Contains("VendorAddress", json);
        Assert.Contains("LineItems", json);
        Assert.Contains("123 Main St", json);
        Assert.Contains("Widget A", json);
        Assert.Contains("object", json);
        Assert.Contains("array", json);

        var result = await response.Content.ReadFromJsonAsync<AnalysisResponse>();
        Assert.NotNull(result);
        Assert.Equal(5, result.Fields!.Count);
        Assert.Equal("string", result.Fields["VendorName"].Type);
        Assert.Equal("object", result.Fields["VendorAddress"].Type);
        Assert.Equal("array", result.Fields["LineItems"].Type);
        Assert.Equal("number", result.Fields["InvoiceTotal"].Type);
        Assert.Equal("boolean", result.Fields["IsVerified"].Type);
    }

    [Theory]
    [InlineData("utility-bill", "utility-bill-001", "electric-bill.pdf")]
    [InlineData("receipt", "receipt-001", "grocery-receipt.pdf")]
    [InlineData("custom", "custom-001", "custom-doc.pdf")]
    public async Task Post_Analyze_WithScenario_ReturnsOkWithMatchingScenario(string scenario, string expectedId, string fileName)
    {
        // Arrange
        var expectedResponse = new AnalysisResponse(
            Id: expectedId,
            Status: "Succeeded",
            FileName: fileName,
            Scenario: scenario,
            AnalyzedAt: DateTimeOffset.UtcNow,
            AnalysisDurationMs: null,
            Fields: new Dictionary<string, FieldResult>
            {
                ["SampleField"] = new FieldResult("SampleField", "value", 0.90f, "string")
            });

        _fixture.MockService
            .AnalyzeAsync(Arg.Any<Stream>(), Arg.Is(fileName), Arg.Any<string>(), Arg.Is(scenario), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        var formContent = CreateFileContent("file content"u8.ToArray(), fileName);

        // Act
        var response = await _client.PostAsync($"/api/analyze?scenario={scenario}", formContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AnalysisResponse>();
        Assert.NotNull(result);
        Assert.Equal(expectedId, result.Id);
        Assert.Equal("Succeeded", result.Status);
        Assert.Equal(fileName, result.FileName);
        Assert.Equal(scenario, result.Scenario);
        Assert.NotNull(result.Fields);
    }

    [Fact]
    public async Task Get_AnalyzeById_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        _fixture.MockService
            .GetResultAsync("nonexistent-id", Arg.Any<CancellationToken>())
            .Returns((AnalysisResponse?)null);

        // Act
        var response = await _client.GetAsync("/api/analyze/nonexistent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
