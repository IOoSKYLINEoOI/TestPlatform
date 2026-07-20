namespace TestPlatform.Contracts.Tags.DTOs;

public record TagPageResponse(
    IReadOnlyList<TagResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
