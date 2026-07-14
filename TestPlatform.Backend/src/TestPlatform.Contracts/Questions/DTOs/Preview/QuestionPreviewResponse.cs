using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.Preview;

public abstract record QuestionPreviewResponse(
    Guid Id,
    string Text,
    string? ImageName,
    QuestionTypeDto Type,
    IReadOnlyList<TagResponse> Tags);
