namespace TestPlatform.Contracts.Auditing.DTOs;

public sealed record AuditLogPageResponse(
    IReadOnlyList<AuditLogItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record AuditLogItemResponse(
    Guid Id,
    Guid? UserId,
    string? EmployeeNumber,
    string Method,
    string Path,
    int StatusCode,
    string TraceId,
    DateTime CreatedAt);
