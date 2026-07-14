namespace TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Response;

public record NumberAttemptAnswerResponse(
    Guid QuestionId,
    decimal NumberAnswer)
    : AttemptAnswerResponse(QuestionId);
