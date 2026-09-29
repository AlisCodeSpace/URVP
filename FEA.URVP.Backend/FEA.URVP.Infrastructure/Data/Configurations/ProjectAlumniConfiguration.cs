using FEA.URVP.Domain.Entities.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class ProjectAlumniConfiguration : IEntityTypeConfiguration<ProjectAlumni>
{
    public void Configure(EntityTypeBuilder<ProjectAlumni> builder)
    {
        builder.ToTable("ProjectAlumni");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.SemesterName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.StudentName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.StudentEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.WasConfirmed).IsRequired();
        builder.Property(a => a.ArchivedAt).IsRequired();

        builder.HasOne(a => a.Project)
            .WithMany()
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Semester)
            .WithMany()
            .HasForeignKey(a => a.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.ProjectId, a.SemesterId, a.StudentUserId })
            .IsUnique();

        builder.HasIndex(a => new { a.ProjectId, a.ArchivedAt });
    }
}
