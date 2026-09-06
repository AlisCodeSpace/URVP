using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class NotificationOutboxConfiguration : IEntityTypeConfiguration<NotificationOutbox>
{
    public void Configure(EntityTypeBuilder<NotificationOutbox> builder)
    {
        builder.ToTable("NotificationOutbox");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(NotificationOutbox.EventTypeMaxLength);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(NotificationOutbox.StatusMaxLength)
            .HasDefaultValue(nameof(NotificationOutboxStatus.Pending));

        builder.Property(x => x.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(NotificationOutbox.ErrorMessageMaxLength);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Notification)
            .WithMany()
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.NotificationId)
            .HasDatabaseName("IX_NotificationOutbox_NotificationId");

        builder.HasIndex(x => new { x.Status, x.NextRetryAt, x.CreatedAt })
            .HasDatabaseName("IX_NotificationOutbox_Pending");
    }
}
