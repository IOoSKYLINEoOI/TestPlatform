using System.Text.Json.Serialization;
using TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Response;
using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Contracts.Questions.DTOs.Results;

namespace TestPlatform.Contracts.Attempts.DTOs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TestAttemptDetailsResponse), "test")]
[JsonDerivedType(typeof(ExamAttemptDetailsResponse), "exam")]
public abstract record AttemptDetailsResponse(
    Guid Id,
    AttemptStatusDto Status,
    int CorrectAnswers,
    int TotalQuestions,
    double Percentage);

public sealed record TestAttemptDetailsResponse(
    Guid Id,
    AttemptStatusDto Status,
    int CorrectAnswers,
    int TotalQuestions,
    double Percentage,
    IReadOnlyCollection<TestAttemptQuestionDetailsResponse> Questions)
    : AttemptDetailsResponse(Id, Status, CorrectAnswers, TotalQuestions, Percentage);

public sealed record ExamAttemptDetailsResponse(
    Guid Id,
    AttemptStatusDto Status,
    int CorrectAnswers,
    int TotalQuestions,
    double Percentage,
    decimal EarnedPoints,
    decimal TotalMaxScore,
    bool Passed,
    IReadOnlyCollection<ExamAttemptQuestionDetailsResponse> Questions)
    : AttemptDetailsResponse(Id, Status, CorrectAnswers, TotalQuestions, Percentage);

public sealed record TestAttemptQuestionDetailsResponse(
    int Order,
    AttemptQuestionResultResponse Question,
    AttemptAnswerResponse? UserAnswer);

public sealed record ExamAttemptQuestionDetailsResponse(
    int Order,
    decimal Score,
    AttemptQuestionResultResponse Question,
    AttemptAnswerResponse? UserAnswer);
