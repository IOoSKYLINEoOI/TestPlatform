using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs;

public record QuestionResponse(
    Guid Id,
    string Text,
    int QuestionTypeId,
    int Points,
    string? ImageName,
    IReadOnlyList<TagResponse> Tags,
    IReadOnlyList<AnswerOptionResponse> AnswerOptions);

    public record AnswerOptionResponse(
        Guid Id,
        string Text,
        bool? IsCorrect,
        string? ImageName);