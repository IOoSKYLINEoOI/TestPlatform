using Microsoft.Extensions.Logging.Abstractions;
using TestPlatform.Web.BackgroundServices;
using Xunit;

namespace TestPlatform.Api.IntegrationTests;

public sealed class PeriodicBackgroundServiceTests
{
    [Fact]
    public async Task ExecuteOnceAsync_FailureDoesNotStopFollowingExecution()
    {
        var job = new TestBackgroundService();

        await job.ExecuteOnceForTestAsync();
        await job.ExecuteOnceForTestAsync();

        Assert.Equal(2, job.ExecutionCount);
    }

    private sealed class TestBackgroundService()
        : PeriodicBackgroundService(
            "test-job",
            TimeSpan.FromMinutes(1),
            TimeProvider.System,
            NullLogger.Instance)
    {
        public int ExecutionCount { get; private set; }

        public Task ExecuteOnceForTestAsync()
            => ExecuteOnceAsync(CancellationToken.None);

        protected override Task ExecuteIterationAsync(CancellationToken cancellationToken)
        {
            ExecutionCount++;
            return ExecutionCount == 1
                ? Task.FromException(new InvalidOperationException("Expected test failure."))
                : Task.CompletedTask;
        }
    }
}
