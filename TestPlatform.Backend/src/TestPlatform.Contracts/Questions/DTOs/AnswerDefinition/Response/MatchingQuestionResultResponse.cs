using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

public record MatchingQuestionResultResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags,
    IReadOnlyList<MatchingItemResponse> LeftItems,
    IReadOnlyList<MatchingItemResponse> RightItems,
    IReadOnlyList<MatchingPairDto> MatchingPair)
    : QuestionResultResponse(Id, Text, ImageId, Tags);