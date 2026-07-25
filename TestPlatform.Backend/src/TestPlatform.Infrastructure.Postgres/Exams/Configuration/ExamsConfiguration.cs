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

        builder.Property(x => x.AttemptsLimit).IsRequired();
        builder.Property(x => x.ReviewPolicy).HasConversion<string>().IsRequired();

        builder.Ignore(x => x.TotalQuestions);
        builder.Ignore(x => x.TotalMaxScore);

        builder.OwnsOne(x => x.Schedule, b =>
        {
            b.ToJson();
        });

        builder.OwnsOne(x => x.PassingRule, b =>
        {
            b.ToJson();
        });

        builder.OwnsMany(x => x.Sections, section =>
        {
            section.ToTable("exam_sections");

            section.WithOwner()
                .HasForeignKey("ExamId");

            section.HasKey("ExamId", "Id");
            section.Property(x => x.Id).ValueGeneratedNever();
            section.Property(x => x.Name).HasMaxLength(200).IsRequired();
            section.Property(x => x.QuestionsToSelect).IsRequired();
            section.Property(x => x.ScorePerQuestion).IsRequired();
            section.Ignore(x => x.QuestionIds);
            section.Ignore(x => x.MaxScore);

            section.OwnsMany(x => x.Questions, question =>
            {
                question.ToTable("exam_section_questions");
                question.WithOwner().HasForeignKey("ExamId", "SectionId");
                question.HasKey("ExamId", "SectionId", nameof(ExamSectionQuestion.QuestionId));
                question.Property(x => x.QuestionId).IsRequired();
                question.HasOne<TestPlatform.Core.Questions.Question>()
                    .WithMany()
                    .HasForeignKey(x => x.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        });

        builder.HasIndex(x => x.AuthorId);
        builder.HasIndex(x => x.Status);
    }
}
