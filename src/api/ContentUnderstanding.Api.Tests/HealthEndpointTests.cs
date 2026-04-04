using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ContentUnderstanding.Api.Tests;

public class HealthEndpointTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(ApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Get_Health_ReturnsOkWithHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(body);
        Assert.Equal("healthy", body.Status);
    }

    private record HealthResponse(string Status);
}
