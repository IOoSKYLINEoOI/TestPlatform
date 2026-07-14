using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Contracts.Attempts.DTOs;

public record AttemptResponse(
    Guid Id,
    int TotalQuestions,
    int AnsweredQuestions,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    AttemptStatusDto Status,
    AttemptTypeDto Type);