namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

public record AnswerOptionResultResponse(
    Guid Id,
    string Text,
    string? ImageName,
    bool IsCorrect);