namespace TestPlatform.Contracts.Exams.DTOs;

public sealed record ExamManagementPageResponse(
    IReadOnlyList<ExamManagementItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ExamManagementItemResponse(
    Guid Id,
    string Title,
    string Description,
    string Status,
    int TotalQuestions,
    int TotalMaxScore,
    DateTime CreatedAt,
    DateTime? PublishedAt);
