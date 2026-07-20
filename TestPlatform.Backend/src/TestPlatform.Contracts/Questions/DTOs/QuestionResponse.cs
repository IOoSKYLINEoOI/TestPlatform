using System.Text.Json.Serialization;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Response;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Contracts.Questions.DTOs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ChoiceQuestionResponse), "choice")]
[JsonDerivedType(typeof(TextQuestionResponse), "text")]
[JsonDerivedType(typeof(NumberQuestionResponse), "number")]
[JsonDerivedType(typeof(MatchingQuestionResponse), "matching")]
public abstract record QuestionResponse(
    Guid Id,
    string Text,
    Guid? ImageId,
    IReadOnlyList<TagResponse> Tags);
