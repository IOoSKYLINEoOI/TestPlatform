using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Core.Auditing;

namespace TestPlatform.Infrastructure.Postgres.Auditing.Configurations;

public sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("audit_log");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.EmployeeNumber).HasMaxLength(100);
        builder.Property(item => item.Method).HasMaxLength(10).IsRequired();
        builder.Property(item => item.Path).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.TraceId).HasMaxLength(100).IsRequired();
        builder.HasIndex(item => item.CreatedAt);
        builder.HasIndex(item => item.UserId);
        builder.HasIndex(item => item.EmployeeNumber);
    }
}
