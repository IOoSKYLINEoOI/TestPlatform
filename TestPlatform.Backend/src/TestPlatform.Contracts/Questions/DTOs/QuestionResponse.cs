using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs;

public abstract record QuestionResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    IReadOnlyList<TagResponse> Tags);