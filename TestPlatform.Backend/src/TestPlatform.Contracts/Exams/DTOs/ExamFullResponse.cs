namespace TestPlatform.Contracts.Exams.DTOs;

public record ExamFullResponse(
    Guid Id,
    string Title,
    string Description,
    int? TimeLimitSeconds,
    string? CoverImageName,
    Guid AuthorId,
    string Status,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    ExamScheduleResponse? Schedule,
    PassingRuleResponse? PassingRule,
    IReadOnlyCollection<ExamQuestionResponse> Questions);

public record ExamScheduleResponse(
    DateTime? AvailableFrom,
    DateTime? AvailableTo);

public record PassingRuleResponse(
    int? MinScore,
    double? MinPercent);

public record ExamQuestionResponse(
    Guid QuestionId,
    int Order,
    int Score);