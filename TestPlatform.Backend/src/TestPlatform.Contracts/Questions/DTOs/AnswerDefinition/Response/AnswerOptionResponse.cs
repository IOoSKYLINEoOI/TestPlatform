namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

public record AnswerOptionResponse(
    Guid Id,
    string Text,
    Guid? ImageId);
