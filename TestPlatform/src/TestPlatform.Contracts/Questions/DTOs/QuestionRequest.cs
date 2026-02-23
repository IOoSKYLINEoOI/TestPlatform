namespace TestPlatform.Contracts.Questions.DTOs;

public record QuestionRequest(
    string Text,
    int QuestionTypeId,
    string? ImageUrl,
    int? Points,
    List<AnswerOptionRequest> AnswerOptions);

public record AnswerOptionRequest(
    string Text,
    bool IsCorrect,
    string? ImageUrl);

