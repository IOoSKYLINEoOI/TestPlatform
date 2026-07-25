using System.Text.Json.Serialization;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;
using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.Editor;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ChoiceQuestionEditorResponse), "choice")]
[JsonDerivedType(typeof(TextQuestionEditorResponse), "text")]
[JsonDerivedType(typeof(NumberQuestionEditorResponse), "number")]
[JsonDerivedType(typeof(MatchingQuestionEditorResponse), "matching")]
public abstract record QuestionEditorResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    IReadOnlyList<TagResponse> Tags,
    string? Explanation,
    QuestionStatusDto Status,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ChoiceQuestionEditorResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    ChoiceModeDto Mode,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags,
    string? Explanation,
    QuestionStatusDto Status,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<AnswerOptionResultResponse> Options)
    : QuestionEditorResponse(Id, Text, ImageId, Tags, Explanation, Status, CreatedByUserId, CreatedAt, UpdatedAt);

public sealed record TextQuestionEditorResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    IReadOnlyList<TagResponse> Tags,
    string? Explanation,
    QuestionStatusDto Status,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CorrectAnswer)
    : QuestionEditorResponse(Id, Text, ImageId, Tags, Explanation, Status, CreatedByUserId, CreatedAt, UpdatedAt);

public sealed record NumberQuestionEditorResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    IReadOnlyList<TagResponse> Tags,
    string? Explanation,
    QuestionStatusDto Status,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    decimal CorrectAnswer)
    : QuestionEditorResponse(Id, Text, ImageId, Tags, Explanation, Status, CreatedByUserId, CreatedAt, UpdatedAt);

public sealed record MatchingQuestionEditorResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags,
    string? Explanation,
    QuestionStatusDto Status,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<MatchingItemResponse> LeftItems,
    IReadOnlyList<MatchingItemResponse> RightItems,
    IReadOnlyList<MatchingPairDto> Pairs)
    : QuestionEditorResponse(Id, Text, ImageId, Tags, Explanation, Status, CreatedByUserId, CreatedAt, UpdatedAt);
