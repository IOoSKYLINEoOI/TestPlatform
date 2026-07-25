namespace TestPlatform.Core.Attempts;

public record AttemptResult(int CorrectAnswers, decimal EarnedPoints, bool? Passed);
