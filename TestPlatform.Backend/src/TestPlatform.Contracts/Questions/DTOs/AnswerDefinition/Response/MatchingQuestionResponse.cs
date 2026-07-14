using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

public record MatchingQuestionResponse(
    Guid Id,
    string Text,
    string? ImageName,
    QuestionTypeDto Type,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags,
    IReadOnlyList<MatchingItemResponse> LeftItems,
    IReadOnlyList<MatchingItemResponse> RightItems)
    : QuestionResponse(Id, Text, ImageName, Tags);