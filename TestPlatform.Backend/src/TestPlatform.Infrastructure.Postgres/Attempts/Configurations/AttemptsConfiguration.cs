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

        builder.Property(a => a.EarnedPoints)
            .HasColumnType("numeric(5,2)")
            .IsRequired();

        builder.Property(a => a.MaxPoints)
            .HasColumnType("numeric(5,2)")
            .IsRequired();

        builder.Property(a => a.StartedAt)
            .IsRequired();

        builder.Property(a => a.FinishedAt);

        builder
            .Property(x => x.Status)
            .HasConversion<string>();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(e => new { e.Type, e.SourceId });

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_Attempt_Parent",
                "(\"TestId\" IS NOT NULL AND \"ExamId\" IS NULL) OR (\"TestId\" IS NULL AND \"ExamId\" IS NOT NULL)"));
    }
}