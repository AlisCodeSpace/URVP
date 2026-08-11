using FEA.URVP.Domain.Entities.ValueLists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class ValueListItemConfiguration : IEntityTypeConfiguration<ValueListItem>
{
    public void Configure(EntityTypeBuilder<ValueListItem> builder)
    {
        builder.ToTable("ValueListItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Kind).IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => new { x.Kind, x.Name })
            .IsUnique()
            .HasDatabaseName("IX_ValueListItems_Kind_Name");

        builder.HasIndex(x => new { x.Kind, x.SortOrder })
            .HasDatabaseName("IX_ValueListItems_Kind_SortOrder");
    }
}
