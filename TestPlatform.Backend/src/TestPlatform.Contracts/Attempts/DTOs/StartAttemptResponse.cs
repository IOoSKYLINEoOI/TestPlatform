using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Contracts.Questions.DTOs.Passing;

namespace TestPlatform.Contracts.Attempts.DTOs;

public record StartAttemptResponse(
    Guid AttemptId,
    int AttemptNumber,
    AttemptStatusDto Status,
    AttemptStartSourceResponse SourceResponse);

public record AttemptStartSourceResponse(
    int? TimeLimitSeconds,
    int TotalQuestions,
    AttemptTypeDto Type,
    IReadOnlyCollection<QuestionAssignmentResponse> Questions);

public record QuestionAssignmentResponse(
    int Order,
    decimal? Score,
    AttemptQuestionResponse Question);
