namespace TestPlatform.Contracts.Exams.DTOs;

public record ExamCatalogPageResponse(
    IReadOnlyList<ExamCatalogItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

public record ExamCatalogItemResponse(
    Guid Id,
    string Title,
    string Description,
    Guid? CoverImageId,
    int? TimeLimitSeconds,
    int AttemptsLimit,
    int TotalQuestions,
    int TotalMaxScore,
    DateTime? AvailableFrom,
    DateTime? AvailableTo);
