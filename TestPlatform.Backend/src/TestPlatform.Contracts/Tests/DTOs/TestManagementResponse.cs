namespace TestPlatform.Contracts.Tests.DTOs;

public sealed record TestManagementPageResponse(
    IReadOnlyList<TestResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
