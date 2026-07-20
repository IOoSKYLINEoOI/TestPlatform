using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Core.Exams;

namespace TestPlatform.Infrastructure.Postgres.Exams.Configuration;

public class ExamsConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("exams");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.TimeLimitSeconds);

        builder.Property(x => x.CoverImageId);

        builder.Property(x => x.AuthorId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.PublishedAt);

        builder.OwnsOne(x => x.Schedule, b =>
        {
            b.ToJson();
        });

        builder.OwnsOne(x => x.PassingRule, b =>
        {
            b.ToJson();
        });

        builder.OwnsMany(x => x.Questions, b =>
        {
            b.ToTable("exam_questions");

            b.WithOwner()
                .HasForeignKey("ExamId");

            b.HasKey("ExamId", "QuestionId");

            b.Property(x => x.QuestionId).IsRequired();
            b.Property(x => x.Order).IsRequired();
            b.Property(x => x.Score).IsRequired();

            b.HasIndex("ExamId", "QuestionId").IsUnique();
            b.HasIndex("ExamId", "Order").IsUnique();
        });

        builder.HasIndex(x => x.AuthorId);
        builder.HasIndex(x => x.Status);
    }
}