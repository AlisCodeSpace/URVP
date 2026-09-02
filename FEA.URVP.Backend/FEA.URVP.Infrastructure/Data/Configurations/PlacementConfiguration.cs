using FEA.URVP.Domain.Entities.Matching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class PlacementConfiguration : IEntityTypeConfiguration<Placement>
{
    public void Configure(EntityTypeBuilder<Placement> builder)
    {
        builder.ToTable("Placements");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.StudentRank).IsRequired();
        builder.Property(p => p.FacultyRank).IsRequired();
        builder.Property(p => p.ResolvedByTieBreak).IsRequired();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.Ignore(p => p.OccupiesSeat);

        builder.HasOne(p => p.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.StudentUser)
            .WithMany()
            .HasForeignKey(p => p.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // A run places each student at most once.
        builder.HasIndex(p => new { p.MatchingRunId, p.StudentUserId }).IsUnique();

        // Seat accounting and "is this student already placed" lookups.
        builder.HasIndex(p => new { p.ProjectId, p.Status });
        builder.HasIndex(p => new { p.StudentUserId, p.Status });
    }
}
