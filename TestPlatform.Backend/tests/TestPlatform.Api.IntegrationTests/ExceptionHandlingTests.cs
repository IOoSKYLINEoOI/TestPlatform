using System.Net;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TestPlatform.Api.IntegrationTests.Infrastructure;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Features.GetMyAttemptsQuery;
using TestPlatform.Contracts.Attempts.DTOs;
using Xunit;

namespace TestPlatform.Api.IntegrationTests;

public sealed class ExceptionHandlingTests(TestPlatformWebApplicationFactory factory)
    : IClassFixture<TestPlatformWebApplicationFactory>
{
    [Fact]
    public async Task UnhandledException_ReturnsSafeProblemDetails()
    {
        using var throwingFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<
                    IQueryHandler<GetMyAttemptsQuery, AttemptHistoryPageResponse>>();
                services.AddScoped<
                    IQueryHandler<GetMyAttemptsQuery, AttemptHistoryPageResponse>,
                    ThrowingAttemptsHandler>();
            }));
        using var client = throwingFactory.CreateClient();

        var response = await client.GetAsync("/attempts");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Sensitive database detail", body, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(body);
        var problem = document.RootElement;
        Assert.Equal(
            "An unexpected server error occurred.",
            problem.GetProperty("title").GetString());
        Assert.Equal(
            "server.unexpected_error",
            problem.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(
            problem.GetProperty("traceId").GetString()));
    }

    private sealed class ThrowingAttemptsHandler
        : IQueryHandler<GetMyAttemptsQuery, AttemptHistoryPageResponse>
    {
        public Task<Result<AttemptHistoryPageResponse>> Handle(
            GetMyAttemptsQuery query,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Sensitive database detail");
    }
}
