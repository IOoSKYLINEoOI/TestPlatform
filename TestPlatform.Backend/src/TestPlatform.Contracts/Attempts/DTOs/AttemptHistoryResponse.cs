using System.Text.Json.Serialization;
using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Contracts.Attempts.DTOs;

public record AttemptHistoryPageResponse(
    IReadOnlyList<AttemptHistoryItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TestAttemptHistoryItemResponse), "test")]
[JsonDerivedType(typeof(ExamAttemptHistoryItemResponse), "exam")]
public abstract record AttemptHistoryItemResponse(
    Guid Id,
    int AttemptNumber,
    Guid SourceId,
    string SourceTitle,
    AttemptStatusDto Status,
    int TotalQuestions,
    int AnsweredQuestions,
    DateTime? StartedAt,
    DateTime? FinishedAt);

public sealed record TestAttemptHistoryItemResponse(
    Guid Id,
    int AttemptNumber,
    Guid SourceId,
    string SourceTitle,
    AttemptStatusDto Status,
    int TotalQuestions,
    int AnsweredQuestions,
    int? CorrectAnswers,
    double? Percentage,
    DateTime? StartedAt,
    DateTime? FinishedAt)
    : AttemptHistoryItemResponse(
        Id, AttemptNumber, SourceId, SourceTitle, Status, TotalQuestions, AnsweredQuestions, StartedAt, FinishedAt);

public sealed record ExamAttemptHistoryItemResponse(
    Guid Id,
    int AttemptNumber,
    Guid SourceId,
    string SourceTitle,
    AttemptStatusDto Status,
    int TotalQuestions,
    int AnsweredQuestions,
    int? CorrectAnswers,
    decimal? EarnedPoints,
    decimal MaxPoints,
    double? Percentage,
    bool? Passed,
    DateTime? StartedAt,
    DateTime? FinishedAt)
    : AttemptHistoryItemResponse(
        Id, AttemptNumber, SourceId, SourceTitle, Status, TotalQuestions, AnsweredQuestions, StartedAt, FinishedAt);

public record ExamAttemptsPageResponse(
    IReadOnlyList<ExamAttemptListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

public record ExamAttemptListItemResponse(
    Guid AttemptId,
    int AttemptNumber,
    Guid UserId,
    string EmployeeNumber,
    AttemptStatusDto Status,
    int TotalQuestions,
    int AnsweredQuestions,
    decimal? EarnedPoints,
    decimal MaxPoints,
    double? Percentage,
    bool? Passed,
    DateTime? StartedAt,
    DateTime? FinishedAt);

public record TestAttemptsPageResponse(
    IReadOnlyList<TestAttemptListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

public record TestAttemptListItemResponse(
    Guid AttemptId,
    int AttemptNumber,
    Guid UserId,
    string EmployeeNumber,
    AttemptStatusDto Status,
    int TotalQuestions,
    int AnsweredQuestions,
    int? CorrectAnswers,
    double? Percentage,
    DateTime? StartedAt,
    DateTime? FinishedAt);
