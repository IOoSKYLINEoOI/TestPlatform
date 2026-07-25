namespace TestPlatform.Web.BackgroundServices;

public sealed class AttemptExpirationOptions
{
    public const string SectionName = "AttemptExpiration";

    public int IntervalSeconds { get; set; } = 60;
}
