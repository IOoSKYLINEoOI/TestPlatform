using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.Preview;

public record MatchingQuestionPreviewResponse(
    Guid Id,
    string Text,
    string? ImageName,
    QuestionTypeDto Type,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags)
    : QuestionPreviewResponse(Id, Text, ImageName, Type, Tags);
