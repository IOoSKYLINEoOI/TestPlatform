using TestPlatform.Contracts.Questions.Enums;

namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

public record MatchingQuestionRequest(
    string Text,
    string? ImageName,
    EvaluationModeDto EvaluationMode,
    List<Guid> TagIds,
    List<MatchingItemRequest> LeftItems,
    List<MatchingItemRequest> RightItems,
    List<MatchingPairDto> Pairs)
    : QuestionRequest(Text, ImageName, TagIds);