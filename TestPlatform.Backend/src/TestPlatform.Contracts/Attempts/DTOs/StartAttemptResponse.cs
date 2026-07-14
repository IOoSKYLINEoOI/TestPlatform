using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Contracts.Attempts.DTOs;

public record StartAttemptResponse(Guid AttemptId, AttemptStartSourceResponse SourceResponse);

public record AttemptStartSourceResponse(
    int? TimeLimitSeconds,
    int TotalQuestions,
    AttemptTypeDto Type,
    IReadOnlyCollection<QuestionAssignmentResponse> Questions);

public record QuestionAssignmentResponse(
    int Order,
    decimal Score,
    QuestionResponse Question);