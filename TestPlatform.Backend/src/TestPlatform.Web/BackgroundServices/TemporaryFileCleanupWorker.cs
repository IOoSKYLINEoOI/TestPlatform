using Microsoft.Extensions.Options;
using TestPlatform.Application.Files;

namespace TestPlatform.Web.BackgroundServices;

public sealed class TemporaryFileCleanupWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<TemporaryFileCleanupOptions> options,
    TimeProvider timeProvider,
    ILogger<TemporaryFileCleanupWorker> logger)
    : PeriodicBackgroundService(
        "temporary-file-cleanup",
        TimeSpan.FromMinutes(options.Value.IntervalMinutes),
        timeProvider,
        logger)
{
    protected override async Task ExecuteIterationAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        await using var scope = scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ITemporaryFileCleanupService>();
        var cutoff = UtcNow.AddHours(-settings.RetentionHours);
        var result = await service.CleanupAsync(cutoff, settings.BatchSize, cancellationToken);

        if (result.Found > 0)
        {
            logger.LogInformation(
                "Temporary file cleanup finished: {Deleted} deleted, {Failed} failed.",
                result.Deleted,
                result.Failed);
        }
    }
}
