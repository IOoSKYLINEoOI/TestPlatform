using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Contracts.Attempts.DTOs;

public record FinishAttemptResponse(
    Guid Id,
    int TotalQuestions,
    int CorrectAnswers,
    decimal EarnedPoints,
    decimal MaxPoints,
    DateTime StartedAt,
    DateTime FinishedAt,
    AttemptStatusDto Status);