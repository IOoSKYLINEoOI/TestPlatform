namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

public record MatchingItemRequest(
    Guid Id,
    string Text,
    bool IsCorrect,
    Guid? ImageId);
