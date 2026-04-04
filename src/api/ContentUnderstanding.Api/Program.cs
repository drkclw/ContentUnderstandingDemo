using ContentUnderstanding.Api.Endpoints;
using ContentUnderstanding.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IContentAnalysisService, ContentAnalysisService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }))
   .WithName("HealthCheck");

app.MapAnalyzeEndpoints();

app.Run();

// Make the implicit Program class accessible to the test project
public partial class Program { }

