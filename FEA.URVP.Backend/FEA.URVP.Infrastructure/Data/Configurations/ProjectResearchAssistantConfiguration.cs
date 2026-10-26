using FEA.URVP.Domain.Entities.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class ProjectResearchAssistantConfiguration : IEntityTypeConfiguration<ProjectResearchAssistant>
{
    public void Configure(EntityTypeBuilder<ProjectResearchAssistant> builder)
    {
        builder.ToTable("ProjectResearchAssistants");

        builder.HasKey(assistant => assistant.Id);

        builder.Property(assistant => assistant.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(assistant => assistant.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(assistant => assistant.CreatedAt).IsRequired();

        builder.HasOne(assistant => assistant.Project)
            .WithMany()
            .HasForeignKey(assistant => assistant.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(assistant => assistant.User)
            .WithMany()
            .HasForeignKey(assistant => assistant.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(assistant => assistant.AddedByUser)
            .WithMany()
            .HasForeignKey(assistant => assistant.AddedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(assistant => assistant.ProjectId);

        builder.HasIndex(assistant => new { assistant.ProjectId, assistant.Email })
            .IsUnique();

        builder.HasIndex(assistant => new { assistant.ProjectId, assistant.UserId })
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");
    }
}
