namespace TestPlatform.Core.Auditing;

public sealed class AuditLogEntry
{
    private AuditLogEntry() { }

    public AuditLogEntry(
        Guid id,
        Guid? userId,
        string? employeeNumber,
        string method,
        string path,
        int statusCode,
        string traceId,
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        EmployeeNumber = employeeNumber;
        Method = method;
        Path = path;
        StatusCode = statusCode;
        TraceId = traceId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public Guid? UserId { get; }
    public string? EmployeeNumber { get; }
    public string Method { get; } = null!;
    public string Path { get; } = null!;
    public int StatusCode { get; }
    public string TraceId { get; } = null!;
    public DateTime CreatedAt { get; }
}
