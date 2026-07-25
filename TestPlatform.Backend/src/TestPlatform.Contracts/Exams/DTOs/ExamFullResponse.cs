namespace TestPlatform.Contracts.Exams.DTOs;

public record ExamFullResponse(
    Guid Id,
    string Title,
    string Description,
    int? TimeLimitSeconds,
    Guid? CoverImageId,
    Guid AuthorId,
    string Status,
    int AttemptsLimit,
    ExamReviewPolicyDto ReviewPolicy,
    int TotalQuestions,
    int TotalMaxScore,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    ExamScheduleResponse? Schedule,
    PassingRuleResponse? PassingRule,
    IReadOnlyCollection<ExamSectionResponse> Sections);

public record ExamScheduleResponse(DateTime? AvailableFrom, DateTime? AvailableTo);

public record PassingRuleResponse(int? MinScore, double? MinPercent);

public record ExamSectionResponse(
    Guid Id,
    string Name,
    int QuestionsToSelect,
    int ScorePerQuestion,
    int MaxScore,
    IReadOnlyCollection<Guid> QuestionIds);
