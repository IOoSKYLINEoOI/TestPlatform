using System.Text.Json.Serialization;
using TestPlatform.Contracts.Questions.Enums;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.Preview;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ChoiceQuestionPreviewResponse), "choice")]
[JsonDerivedType(typeof(TextQuestionPreviewResponse), "text")]
[JsonDerivedType(typeof(NumberQuestionPreviewResponse), "number")]
[JsonDerivedType(typeof(MatchingQuestionPreviewResponse), "matching")]
public abstract record QuestionPreviewResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    QuestionTypeDto Type,
    IReadOnlyList<TagResponse> Tags);
