namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

public record MatchingItemRequest(
    string Text,
    bool IsCorrect,
    string? ImageUrl);
