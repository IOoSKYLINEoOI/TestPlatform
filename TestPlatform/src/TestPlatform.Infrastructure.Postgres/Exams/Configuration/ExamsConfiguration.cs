using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Infrastructure.Postgres.Exams.Entities;

namespace TestPlatform.Infrastructure.Postgres.Exams.Configuration;

public class ExamsConfiguration : IEntityTypeConfiguration<ExamEntity>
{
    public void Configure(EntityTypeBuilder<ExamEntity> builder)
    {
        builder.ToTable("exams");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(e => e.TimeLimitSeconds);

        builder.HasMany(e => e.Questions)
            .WithMany(q => q.Exams)
            .UsingEntity(j => j.ToTable("exams_questions"));
    }
}