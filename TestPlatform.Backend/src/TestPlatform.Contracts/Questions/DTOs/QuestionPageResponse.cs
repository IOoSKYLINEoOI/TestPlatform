using TestPlatform.Contracts.Questions.DTOs.Preview;

namespace TestPlatform.Contracts.Questions.DTOs;

public record QuestionPageResponse(
    IReadOnlyList<QuestionPreviewResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
