namespace TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Response;

public record ChoiceAttemptAnswerResponse(
    Guid QuestionId,
    IReadOnlyCollection<Guid> SelectedOptionIds)
    : AttemptAnswerResponse(QuestionId);
