using TestPlatform.Core.Auditing;

namespace TestPlatform.Application.Auditing;

public interface IAuditLogReadDbContext
{
    IQueryable<AuditLogEntry> ReadAuditLog { get; }
}
