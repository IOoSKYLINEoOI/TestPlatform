namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

public record AnswerOptionRequest(
    string Text,
    bool IsCorrect,
    Guid? ImageId);