using System.Diagnostics.Metrics;

namespace TestPlatform.Web.BackgroundServices;

public static class BackgroundJobMetrics
{
    public const string MeterName = "TestPlatform.BackgroundJobs";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> Executions = Meter.CreateCounter<long>(
        "background_job.executions",
        description: "Number of background job executions.");
    private static readonly Counter<long> Failures = Meter.CreateCounter<long>(
        "background_job.failures",
        description: "Number of failed background job executions.");
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>(
        "background_job.duration",
        unit: "ms",
        description: "Background job execution duration.");

    public static void RecordExecution(string jobName, double durationMilliseconds)
    {
        var tag = new KeyValuePair<string, object?>("job.name", jobName);
        Executions.Add(1, tag);
        Duration.Record(durationMilliseconds, tag);
    }

    public static void RecordFailure(string jobName)
    {
        Failures.Add(1, new KeyValuePair<string, object?>("job.name", jobName));
    }
}
