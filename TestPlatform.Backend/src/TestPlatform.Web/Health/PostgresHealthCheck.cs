using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TestPlatform.Infrastructure.Postgres;

namespace TestPlatform.Web.Health;

public sealed class PostgresHealthCheck(TestPlatformDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await dbContext.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Unhealthy("PostgreSQL is unavailable.");
            }

            var pendingMigrations = (await dbContext.Database
                    .GetPendingMigrationsAsync(cancellationToken))
                .ToArray();

            return pendingMigrations.Length == 0
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy(
                    "PostgreSQL schema has pending migrations.",
                    data: new Dictionary<string, object>
                    {
                        ["pendingMigrations"] = pendingMigrations,
                    });
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL health check failed.", exception);
        }
    }
}
