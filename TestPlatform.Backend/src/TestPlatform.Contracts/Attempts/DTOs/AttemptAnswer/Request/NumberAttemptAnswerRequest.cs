namespace TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Request;

public record NumberAttemptAnswerRequest(
    Guid QuestionId,
    decimal NumberAnswer)
    : AttemptAnswerRequest(QuestionId);
