using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

public record ChoiceQuestionResultResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    ChoiceModeDto Mode,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags,
    IReadOnlyList<AnswerOptionResultResponse> Options)
    : QuestionResultResponse(Id, Text, ImageId, Tags);