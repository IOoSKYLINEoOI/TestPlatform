using System.Text.Json.Serialization;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;
using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.Passing;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ChoiceAttemptQuestionResponse), "choice")]
[JsonDerivedType(typeof(TextAttemptQuestionResponse), "text")]
[JsonDerivedType(typeof(NumberAttemptQuestionResponse), "number")]
[JsonDerivedType(typeof(MatchingAttemptQuestionResponse), "matching")]
public abstract record AttemptQuestionResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    IReadOnlyList<TagResponse> Tags);

public sealed record ChoiceAttemptQuestionResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    ChoiceModeDto Mode,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags,
    IReadOnlyList<AnswerOptionResponse> Options)
    : AttemptQuestionResponse(Id, Text, ImageId, Tags);

public sealed record TextAttemptQuestionResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    IReadOnlyList<TagResponse> Tags)
    : AttemptQuestionResponse(Id, Text, ImageId, Tags);

public sealed record NumberAttemptQuestionResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    IReadOnlyList<TagResponse> Tags)
    : AttemptQuestionResponse(Id, Text, ImageId, Tags);

public sealed record MatchingAttemptQuestionResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    EvaluationModeDto EvaluationMode,
    IReadOnlyList<TagResponse> Tags,
    IReadOnlyList<MatchingItemResponse> LeftItems,
    IReadOnlyList<MatchingItemResponse> RightItems)
    : AttemptQuestionResponse(Id, Text, ImageId, Tags);
