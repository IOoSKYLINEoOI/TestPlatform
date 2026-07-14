namespace TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Response;

public record TextAttemptAnswerResponse(
    Guid QuestionId,
    string TextAnswer)
    : AttemptAnswerResponse(QuestionId);
