using System.Diagnostics;

namespace TestPlatform.Web.BackgroundServices;

public abstract class PeriodicBackgroundService(
    string jobName,
    TimeSpan interval,
    TimeProvider timeProvider,
    ILogger logger) : BackgroundService
{
    protected DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    protected abstract Task ExecuteIterationAsync(CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(interval, timeProvider);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ExecuteOnceAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    protected async Task ExecuteOnceAsync(CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.GetTimestamp();
        try
        {
            await ExecuteIterationAsync(cancellationToken);
            var duration = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            BackgroundJobMetrics.RecordExecution(jobName, duration);
            logger.LogDebug(
                "Background job {JobName} completed in {DurationMilliseconds:F0} ms.",
                jobName,
                duration);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            var duration = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            BackgroundJobMetrics.RecordExecution(jobName, duration);
            BackgroundJobMetrics.RecordFailure(jobName);
            logger.LogError(
                exception,
                "Background job {JobName} failed after {DurationMilliseconds:F0} ms.",
                jobName,
                duration);
        }
    }
}
