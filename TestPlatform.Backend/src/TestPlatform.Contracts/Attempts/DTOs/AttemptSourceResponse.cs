using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Contracts.Attempts.DTOs;

public record AttemptSourceResponse(
    int TotalQuestions,
    int? TimeLimitSeconds,
    AttemptTypeDto Type,
    IReadOnlyCollection<QuestionResponse> Questions);
