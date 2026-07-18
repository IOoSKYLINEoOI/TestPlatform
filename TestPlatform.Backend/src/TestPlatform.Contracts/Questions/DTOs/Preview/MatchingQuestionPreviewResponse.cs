using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.Preview;

public record MatchingQuestionPreviewResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags)
    : QuestionPreviewResponse(Id, Text, ImageId, Type, Tags);