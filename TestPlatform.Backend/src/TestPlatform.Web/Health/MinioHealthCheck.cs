using Microsoft.Extensions.Diagnostics.HealthChecks;
using Minio;

namespace TestPlatform.Web.Health;

public sealed class MinioHealthCheck(IMinioClient minioClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await minioClient.ListBucketsAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("MinIO health check failed.", exception);
        }
    }
}
