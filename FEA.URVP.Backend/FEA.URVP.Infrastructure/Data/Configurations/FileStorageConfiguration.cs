using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class FileStorageConfiguration : IEntityTypeConfiguration<FileStorage>
{
    public void Configure(EntityTypeBuilder<FileStorage> builder)
    {
        builder.ToTable("FileStorage");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.FileCategory)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.FileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(f => f.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.FileSize)
            .IsRequired();

        builder.Property(f => f.ContentHash)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(f => f.Content)
            .IsRequired();

        builder.Property(f => f.UploadedAt)
            .IsRequired();

        builder.Property(f => f.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(f => new { f.EntityType, f.EntityId, f.FileCategory, f.IsDeleted })
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_FileStorage_Entity");

        builder.HasIndex(f => f.ContentHash)
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_FileStorage_ContentHash");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_FileStorage_EntityType",
                $"[EntityType] IN ('{FileStorageCatalog.EntityStudentProfile}')");

            t.HasCheckConstraint(
                "CK_FileStorage_FileSize",
                $"([FileCategory] IN ('{FileStorageCatalog.CategoryTranscript}', '{FileStorageCatalog.CategoryCitiCertification}') AND [FileSize] <= {FileStorageCatalog.MaxDocumentBytes})");
        });
    }
}
