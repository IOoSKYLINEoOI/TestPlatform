using TestPlatform.Contracts.Questions.Enums;

namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

public record ChoiceQuestionRequest(
    string Text,
    string? ImageName,
    ChoiceModeDto Mode,
    EvaluationModeDto EvaluationMode,
    List<Guid> TagIds,
    List<AnswerOptionRequest> Options)
    : QuestionRequest(Text, ImageName, TagIds);