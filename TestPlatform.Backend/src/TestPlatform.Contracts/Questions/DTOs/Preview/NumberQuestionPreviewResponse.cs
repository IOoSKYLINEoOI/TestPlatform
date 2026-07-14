using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.Preview;

public record NumberQuestionPreviewResponse(
    Guid Id,
    string Text,
    string? ImageName,
    QuestionTypeDto Type,
    IReadOnlyList<TagResponse> Tags)
    : QuestionPreviewResponse(Id, Text, ImageName, Type, Tags);