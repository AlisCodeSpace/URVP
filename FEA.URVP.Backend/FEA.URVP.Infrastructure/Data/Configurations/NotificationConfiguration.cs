using FEA.URVP.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(Notification.TypeMaxLength);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(Notification.TitleMaxLength);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(Notification.MessageMaxLength);

        builder.Property(x => x.Data)
            .HasMaxLength(Notification.DataMaxLength);

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(Notification.ReferenceTypeMaxLength);

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasMaxLength(Notification.PriorityMaxLength)
            .HasDefaultValue(Notification.DefaultPriority);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Notifications_UserId_CreatedAt");

        builder.HasIndex(x => new { x.UserId, x.IsRead })
            .HasDatabaseName("IX_Notifications_UserId_IsRead");

        builder.HasIndex(x => new { x.UserId, x.Type, x.ReferenceId })
            .IsUnique()
            .HasFilter("[ReferenceId] IS NOT NULL")
            .HasDatabaseName("IX_Notifications_UserId_Type_ReferenceId");
    }
}
