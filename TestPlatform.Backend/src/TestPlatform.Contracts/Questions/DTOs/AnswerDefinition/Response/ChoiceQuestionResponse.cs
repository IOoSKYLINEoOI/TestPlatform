using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

public record ChoiceQuestionResponse(
    Guid Id,
    string Text,
    string? ImageName,
    QuestionTypeDto Type,
    ChoiceModeDto Mode,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags,
    IReadOnlyList<AnswerOptionResponse> Options)
    : QuestionResponse(Id, Text, ImageName, Tags);
