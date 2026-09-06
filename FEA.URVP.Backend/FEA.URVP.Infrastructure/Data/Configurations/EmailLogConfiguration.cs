using FEA.URVP.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("EmailLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.From)
            .IsRequired()
            .HasMaxLength(EmailLog.AddressMaxLength);

        builder.Property(x => x.To)
            .IsRequired()
            .HasMaxLength(EmailLog.AddressMaxLength);

        builder.Property(x => x.Cc)
            .HasMaxLength(EmailLog.RecipientsMaxLength);

        builder.Property(x => x.Bcc)
            .HasMaxLength(EmailLog.RecipientsMaxLength);

        builder.Property(x => x.Body)
            .IsRequired();

        builder.Property(x => x.Exception)
            .HasMaxLength(EmailLog.ExceptionMaxLength);

        builder.Property(x => x.Success)
            .IsRequired();

        builder.Property(x => x.CreatedOn)
            .IsRequired();

        builder.Property(x => x.ModifiedOn)
            .IsRequired();

        builder.HasIndex(x => x.CreatedOn)
            .HasDatabaseName("IX_EmailLogs_CreatedOn");
    }
}
