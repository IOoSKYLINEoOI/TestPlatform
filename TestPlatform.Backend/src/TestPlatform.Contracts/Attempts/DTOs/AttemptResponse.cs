using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Contracts.Attempts.DTOs;

public record AttemptResponse(
    Guid Id,
    int TotalQuestions,
    int? CorrectAnswers,
    decimal EarnedPoints,
    decimal MaxPoints,
    Guid UserId,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    AttemptStatusDto Status,
    AttemptTypeDto Type,
    Guid SourceId);