namespace TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Request;

public record TextAttemptAnswerRequest(
    Guid QuestionId,
    string TextAnswer)
    : AttemptAnswerRequest(QuestionId);