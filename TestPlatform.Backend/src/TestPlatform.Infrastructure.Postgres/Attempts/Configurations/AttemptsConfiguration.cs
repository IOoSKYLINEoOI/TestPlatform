using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Infrastructure.Postgres.Attempts.Entities;

namespace TestPlatform.Infrastructure.Postgres.Attempts.Configurations;

public class AttemptsConfiguration : IEntityTypeConfiguration<AttemptEntity>
{
    public void Configure(EntityTypeBuilder<AttemptEntity> builder)
    {
        builder.ToTable("attempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TotalQuestions)
            .IsRequired();

        builder.Property(a => a.CorrectAnswers)
            .IsRequired();

        builder.Property(a => a.Score)
            .IsRequired();

        builder.Property(a => a.StartedAt)
            .IsRequired();

        builder.Property(a => a.FinishedAt);

        builder.Property(a => a.ParentId)
            .IsRequired();

        builder.Property(a => a.ParentType)
            .IsRequired();

        builder.HasIndex(a => new { a.ParentType, a.ParentId });
    }
}