namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

public record MatchingItemResponse(
    Guid Id,
    string Text,
    Guid? ImageId);
