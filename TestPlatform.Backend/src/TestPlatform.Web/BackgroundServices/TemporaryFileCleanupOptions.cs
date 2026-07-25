namespace TestPlatform.Web.BackgroundServices;

public sealed class TemporaryFileCleanupOptions
{
    public const string SectionName = "TemporaryFileCleanup";

    public int RetentionHours { get; set; } = 24;

    public int IntervalMinutes { get; set; } = 30;

    public int BatchSize { get; set; } = 100;
}
