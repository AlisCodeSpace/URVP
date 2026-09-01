using FEA.URVP.Domain.Entities.Workshops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class WorkshopConfiguration : IEntityTypeConfiguration<Workshop>
{
    public void Configure(EntityTypeBuilder<Workshop> builder)
    {
        builder.ToTable("Workshops");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Date)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Time)
            .HasMaxLength(64);

        builder.Property(x => x.Location)
            .HasMaxLength(256);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.RegistrationUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.PosterAlt)
            .HasMaxLength(256);

        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.SortOrder)
            .HasDatabaseName("IX_Workshops_SortOrder");
    }
}
