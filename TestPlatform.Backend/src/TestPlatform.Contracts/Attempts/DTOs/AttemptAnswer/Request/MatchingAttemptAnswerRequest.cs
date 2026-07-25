namespace TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Request;

public record MatchingAttemptAnswerRequest(
    Guid QuestionId,
    IReadOnlyList<AttemptMatchingPairRequest> MatchingPairs)
    : AttemptAnswerRequest(QuestionId);

public record AttemptMatchingPairRequest(
    Guid LeftOptionId,
    Guid RightOptionId);
