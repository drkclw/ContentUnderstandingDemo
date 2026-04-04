using ContentUnderstanding.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace ContentUnderstanding.Api.Tests;

public class ApiFixture : WebApplicationFactory<Program>
{
    public IContentAnalysisService MockService { get; } = Substitute.For<IContentAnalysisService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real service registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IContentAnalysisService));

            if (descriptor is not null)
                services.Remove(descriptor);

            // Register the mock
            services.AddSingleton(MockService);
        });
    }
}
