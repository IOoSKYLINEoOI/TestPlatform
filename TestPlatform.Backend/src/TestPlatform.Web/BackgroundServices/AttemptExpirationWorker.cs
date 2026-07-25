using Microsoft.Extensions.Options;
using TestPlatform.Application.Attempts;

namespace TestPlatform.Web.BackgroundServices;

public sealed class AttemptExpirationWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<AttemptExpirationOptions> options,
    TimeProvider timeProvider,
    ILogger<AttemptExpirationWorker> logger)
    : PeriodicBackgroundService(
        "attempt-expiration",
        TimeSpan.FromSeconds(options.Value.IntervalSeconds),
        timeProvider,
        logger)
{
    protected override async Task ExecuteIterationAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IAttemptExpirationService>();
        var count = await service.ExpireOverdueAsync(
            UtcNow,
            cancellationToken);

        if (count > 0)
        {
            logger.LogInformation("Expired {AttemptCount} overdue attempts.", count);
        }
    }
}
