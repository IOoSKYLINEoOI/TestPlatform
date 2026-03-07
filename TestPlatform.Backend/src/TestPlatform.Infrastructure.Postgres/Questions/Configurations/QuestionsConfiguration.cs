using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Infrastructure.Postgres.Questions.Entities;

namespace TestPlatform.Infrastructure.Postgres.Questions.Configurations;

public class QuestionsConfiguration: IEntityTypeConfiguration<QuestionEntity>
{
    public void Configure(EntityTypeBuilder<QuestionEntity> builder)
    {
        builder.ToTable("questions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.QuestionTypeId)
            .IsRequired();

        builder.Property(x => x.Points)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500);

        builder.HasMany(x => x.AnswersOptions)
            .WithOne(x => x.Question)
            .HasForeignKey(x => x.QuestionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tags)
            .WithMany(x => x.Questions)
            .UsingEntity(j => j.ToTable("questions_tags"));
    }
}