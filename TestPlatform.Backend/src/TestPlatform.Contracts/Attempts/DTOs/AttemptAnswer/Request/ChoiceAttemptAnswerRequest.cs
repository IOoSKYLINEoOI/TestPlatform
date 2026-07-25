namespace TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Request;

public record ChoiceAttemptAnswerRequest(
    Guid QuestionId,
    IReadOnlyList<Guid> SelectedOptionIds)
    : AttemptAnswerRequest(QuestionId);
