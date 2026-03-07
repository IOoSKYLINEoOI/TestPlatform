using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Infrastructure.Postgres.Questions.Entities;

namespace TestPlatform.Infrastructure.Postgres.Questions.Configurations;

public class AnswerOptionsConfiguration : IEntityTypeConfiguration<AnswerOptionEntity>
{
    public void Configure(EntityTypeBuilder<AnswerOptionEntity> builder)
    {
        builder.ToTable("answer_options");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.IsCorrect)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500);
    }
}