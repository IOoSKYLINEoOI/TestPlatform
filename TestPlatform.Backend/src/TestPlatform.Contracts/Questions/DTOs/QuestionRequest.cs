using System.Text.Json.Serialization;
using TestPlatform.Contracts.Questions.DTOs.AnswerDefinition.Request;

namespace TestPlatform.Contracts.Questions.DTOs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ChoiceQuestionRequest), "choice")]
[JsonDerivedType(typeof(TextQuestionRequest), "text")]
[JsonDerivedType(typeof(NumberQuestionRequest), "number")]
[JsonDerivedType(typeof(MatchingQuestionRequest), "matching")]
public abstract record QuestionRequest(
    string Text,
    Guid? ImageId,
    List<Guid> TagIds);
