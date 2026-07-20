using TestPlatform.Contracts.Questions.DTOs.Preview;

namespace TestPlatform.Contracts.Tags.DTOs;

public record TagPageQuestionsResponse(
    IReadOnlyList<QuestionPreviewResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
