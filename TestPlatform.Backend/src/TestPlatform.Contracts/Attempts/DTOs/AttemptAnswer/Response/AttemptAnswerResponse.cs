using System.Text.Json.Serialization;

namespace TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Response;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ChoiceAttemptAnswerResponse), "choice")]
[JsonDerivedType(typeof(TextAttemptAnswerResponse), "text")]
[JsonDerivedType(typeof(NumberAttemptAnswerResponse), "number")]
[JsonDerivedType(typeof(MatchingAttemptAnswerResponse), "matching")]
public abstract record AttemptAnswerResponse(Guid QuestionId);
