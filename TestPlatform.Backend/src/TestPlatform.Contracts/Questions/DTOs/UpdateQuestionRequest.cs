using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs;

public record UpdateQuestionRequest(
    string Text,
    int QuestionTypeId,
    int Points,
    string? ImageName,
    List<Guid> TagIds,
    List<UpdateAnswerOptionRequest> AnswerOptions);

public record UpdateAnswerOptionRequest(
    Guid? Id,
    string Text,
    bool IsCorrect,
    string? ImageName);
