using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Contracts.Attempts.DTOs;

public sealed record AttemptSourcePageResponse(
    IReadOnlyList<AttemptSourceListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record AttemptSourceListItemResponse(
    Guid Id,
    string Title,
    string Description,
    AttemptTypeDto Type,
    string Status);
