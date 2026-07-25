using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Core.Questions;
using TestPlatform.Infrastructure.Postgres.Questions.Mapping;

namespace TestPlatform.Infrastructure.Postgres.Questions.Configurations;

public class QuestionsConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions");

        builder.HasKey(x => x.Id);

        builder.ComplexProperty(x => x.Content, content =>
        {
            content.Property(x => x.Text)
                .HasColumnName("Text")
                .HasMaxLength(QuestionContent.MaxTextLength)
                .IsRequired();

            content.Property(x => x.Explanation)
                .HasColumnName("Explanation")
                .HasMaxLength(QuestionContent.MaxExplanationLength);
        });

        builder.Property(x => x.CreatedByUserId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Ignore(x => x.QuestionType);

        builder.Property(x => x.ImageId);

        builder.HasMany(x => x.Tags)
            .WithMany()
            .UsingEntity(j => j.ToTable("question_tags"));

        builder.Property(x => x.AnswerDefinition)
            .HasConversion(new AnswerDefinitionValueConverter())
            .HasColumnType("jsonb")
            .IsRequired();
    }
}
