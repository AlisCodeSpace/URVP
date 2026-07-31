using FEA.URVP.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(u => u.Affiliation)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.Role)
            .IsRequired();

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(512);

        builder.Property(u => u.RegisteredAt)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();
    }
}
