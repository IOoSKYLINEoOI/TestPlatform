using System.Text.Json.Serialization;
using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Contracts.Attempts.DTOs;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TestAttemptResultResponse), "test")]
[JsonDerivedType(typeof(ExamAttemptResultResponse), "exam")]
public abstract record AttemptResultResponse(
    Guid Id,
    int TotalQuestions,
    int CorrectAnswers,
    double Percentage,
    DateTime StartedAt,
    DateTime FinishedAt,
    AttemptStatusDto Status);

public sealed record TestAttemptResultResponse(
    Guid Id,
    int TotalQuestions,
    int CorrectAnswers,
    double Percentage,
    DateTime StartedAt,
    DateTime FinishedAt,
    AttemptStatusDto Status)
    : AttemptResultResponse(
        Id,
        TotalQuestions,
        CorrectAnswers,
        Percentage,
        StartedAt,
        FinishedAt,
        Status);

public sealed record ExamAttemptResultResponse(
    Guid Id,
    int TotalQuestions,
    int CorrectAnswers,
    double Percentage,
    decimal EarnedPoints,
    decimal MaxPoints,
    bool Passed,
    DateTime StartedAt,
    DateTime FinishedAt,
    AttemptStatusDto Status)
    : AttemptResultResponse(
        Id,
        TotalQuestions,
        CorrectAnswers,
        Percentage,
        StartedAt,
        FinishedAt,
        Status);
