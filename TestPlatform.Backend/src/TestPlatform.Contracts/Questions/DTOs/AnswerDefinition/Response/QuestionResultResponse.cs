using System.Text.Json.Serialization;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ChoiceQuestionResultResponse), "choice")]
[JsonDerivedType(typeof(TextQuestionResultResponse), "text")]
[JsonDerivedType(typeof(NumberQuestionResultResponse), "number")]
[JsonDerivedType(typeof(MatchingQuestionResultResponse), "matching")]
public abstract record QuestionResultResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    IReadOnlyList<TagResponse> Tags);
