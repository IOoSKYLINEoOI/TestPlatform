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

        builder.Property(x => x.Text)
            .HasMaxLength(200)
            .IsRequired();

        builder.Ignore(x => x.QuestionType);

        builder.Property(x => x.ImageName)
            .HasMaxLength(500);

        builder.HasMany(x => x.Tags)
            .WithMany()
            .UsingEntity(j => j.ToTable("question_tags"));

        builder.Property(x => x.AnswerDefinition)
            .HasConversion(new AnswerDefinitionValueConverter())
            .HasColumnType("jsonb");
    }
}