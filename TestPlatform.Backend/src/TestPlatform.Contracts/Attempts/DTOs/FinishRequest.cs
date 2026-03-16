namespace TestPlatform.Contracts.Attempts.DTOs;

public record FinishRequest(IReadOnlyList<UserAnswer> UserAnswers);