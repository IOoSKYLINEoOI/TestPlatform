using System.Text.Json.Serialization;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;
using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.Results;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ChoiceAttemptQuestionResultResponse), "choice")]
[JsonDerivedType(typeof(TextAttemptQuestionResultResponse), "text")]
[JsonDerivedType(typeof(NumberAttemptQuestionResultResponse), "number")]
[JsonDerivedType(typeof(MatchingAttemptQuestionResultResponse), "matching")]
public abstract record AttemptQuestionResultResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    IReadOnlyList<TagResponse> Tags,
    string? Explanation);

public sealed record ChoiceAttemptQuestionResultResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    ChoiceModeDto Mode,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags,
    string? Explanation,
    IReadOnlyList<AnswerOptionResultResponse> Options)
    : AttemptQuestionResultResponse(Id, Text, ImageId, Tags, Explanation);

public sealed record TextAttemptQuestionResultResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    IReadOnlyList<TagResponse> Tags,
    string? Explanation,
    string CorrectAnswer)
    : AttemptQuestionResultResponse(Id, Text, ImageId, Tags, Explanation);

public sealed record NumberAttemptQuestionResultResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    IReadOnlyList<TagResponse> Tags,
    string? Explanation,
    decimal CorrectAnswer)
    : AttemptQuestionResultResponse(Id, Text, ImageId, Tags, Explanation);

public sealed record MatchingAttemptQuestionResultResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags,
    string? Explanation,
    IReadOnlyList<MatchingItemResponse> LeftItems,
    IReadOnlyList<MatchingItemResponse> RightItems,
    IReadOnlyList<MatchingPairDto> Pairs)
    : AttemptQuestionResultResponse(Id, Text, ImageId, Tags, Explanation);
