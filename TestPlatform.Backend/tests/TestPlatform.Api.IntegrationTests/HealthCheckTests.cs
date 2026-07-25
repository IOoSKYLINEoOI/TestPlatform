using System.Net;
using TestPlatform.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace TestPlatform.Api.IntegrationTests;

public sealed class HealthCheckTests(TestPlatformWebApplicationFactory factory)
    : IClassFixture<TestPlatformWebApplicationFactory>
{
    [Fact]
    public async Task Liveness_DoesNotRequireAuthenticationOrExternalDependencies()
    {
        using var client = factory.CreateAnonymousClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"self\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain("\"postgres\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain("\"minio\"", body, StringComparison.Ordinal);
    }
}
