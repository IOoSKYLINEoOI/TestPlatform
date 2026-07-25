using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Core.Files;

namespace TestPlatform.Infrastructure.Postgres.Files.Configurations;

public class FileAssetsConfiguration : IEntityTypeConfiguration<FileAsset>
{
    public void Configure(EntityTypeBuilder<FileAsset> builder)
    {
        builder.ToTable("file_assets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ObjectKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SizeBytes)
            .IsRequired();

        builder.Property(x => x.UploadedByUserId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.AttachedAt);
        builder.Property(x => x.DeletedAt);

        builder.HasIndex(x => x.ObjectKey).IsUnique();
        builder.HasIndex(x => new { x.UploadedByUserId, x.Status });
    }
}
