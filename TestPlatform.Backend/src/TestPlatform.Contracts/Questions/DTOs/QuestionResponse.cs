using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs;

public abstract record QuestionResponse(
    Guid Id,
    string Text,
    string? ImageName,
    IReadOnlyList<TagResponse> Tags);