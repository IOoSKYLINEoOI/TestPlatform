namespace TestPlatform.Contracts.Questions.DTOs;

public record QuestionRequest(
    string Text,
    int QuestionTypeId,
    int Points,
    string? ImageName,
    List<Guid> TagIds,
    List<CreateAnswerOptionRequest> CreateAnswerOptions);

public record CreateAnswerOptionRequest(
    Guid Id,
    string Text,
    bool IsCorrect,
    string? ImageUrl);
