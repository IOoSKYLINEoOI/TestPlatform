namespace TestPlatform.Core.Attempts;

public record Score(decimal Earned, decimal Max)
{
    public decimal Percent => Max > 0 ? Earned / Max : 0m;
}
