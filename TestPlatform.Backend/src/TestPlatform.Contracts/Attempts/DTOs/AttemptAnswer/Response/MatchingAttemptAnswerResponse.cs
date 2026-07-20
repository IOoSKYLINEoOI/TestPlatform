namespace TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Response;

public record MatchingAttemptAnswerResponse(Guid QuestionId,
    IReadOnlyList<AttemptMatchingPairResponse> MatchingPairs)
    : AttemptAnswerResponse(QuestionId);

public record AttemptMatchingPairResponse(
    Guid LeftOptionId,
    Guid RightOptionId);