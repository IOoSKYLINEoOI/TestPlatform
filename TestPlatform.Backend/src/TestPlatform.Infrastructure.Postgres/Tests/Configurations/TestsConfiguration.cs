using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Core.Tests;

namespace TestPlatform.Infrastructure.Postgres.Tests.Configurations;

public class TestsConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
    {
        builder.ToTable("tests");

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
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.PublishedAt);

        builder.OwnsMany(x => x.Questions, b =>
        {
            b.ToTable("test_questions");

            b.WithOwner()
                .HasForeignKey("TestId");

            b.HasKey("TestId", "QuestionId");

            b.Property(x => x.QuestionId).IsRequired();
            b.HasOne<TestPlatform.Core.Questions.Question>()
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Order).IsRequired();
            b.HasIndex("TestId", "QuestionId").IsUnique();
            b.HasIndex("TestId", "Order").IsUnique();
        });

        builder.HasIndex(x => x.AuthorId);
    }
}
