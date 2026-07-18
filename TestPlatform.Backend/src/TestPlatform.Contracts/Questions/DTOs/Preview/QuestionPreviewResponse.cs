using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.Preview;

public abstract record QuestionPreviewResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    IReadOnlyList<TagResponse> Tags);