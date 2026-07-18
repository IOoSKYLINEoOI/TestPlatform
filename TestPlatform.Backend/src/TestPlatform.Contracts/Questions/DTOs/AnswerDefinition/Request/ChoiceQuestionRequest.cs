using TestPlatform.Contracts.Questions.Enums;

namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

public record ChoiceQuestionRequest(
    string Text,
    Guid? ImageId,
    ChoiceModeDto Mode,
    EvaluationModeDto EvaluationMode,
    List<Guid> TagIds,
    List<AnswerOptionRequest> Options)
    : QuestionRequest(Text, ImageId, TagIds);