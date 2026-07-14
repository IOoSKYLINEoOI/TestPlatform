using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.Preview;

public record ChoiceQuestionPreviewResponse(
    Guid Id,
    string Text,
    string? ImageName,
    QuestionTypeDto Type,
    ChoiceModeDto Mode,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags)
    : QuestionPreviewResponse(Id, Text, ImageName, Type, Tags);