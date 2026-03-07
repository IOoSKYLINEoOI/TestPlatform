using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Tests.DTOs;

public record TestFullResponse(
    Guid Id,
    string Name,
    int? TimeLimitSeconds,
    string Description,
    Guid? AuthorId,
    int TotalQuestions,
    List<TagResponse> Tags,
    List<QuestionFullResponse> Questions);

public record QuestionFullResponse(
    Guid Id,
    string Text,
    int QuestionTypeId,
    int Points,
    string? ImageUrl,
    IReadOnlyList<TagResponse>? Tags,
    IReadOnlyList<AnswerOptionFullResponse> AnswerOptions);

public record AnswerOptionFullResponse(
    Guid Id,
    string Text,
    bool? IsCorrect,
    string? ImageUrl);