using System.Text.Json.Serialization;

namespace TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Request;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ChoiceAttemptAnswerRequest), "choice")]
[JsonDerivedType(typeof(TextAttemptAnswerRequest), "text")]
[JsonDerivedType(typeof(NumberAttemptAnswerRequest), "number")]
[JsonDerivedType(typeof(MatchingAttemptAnswerRequest), "matching")]
public abstract record AttemptAnswerRequest(Guid QuestionId);
