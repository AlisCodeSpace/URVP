using FEA.URVP.Domain.Entities.ProjectRankings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class ProjectRankingConfiguration : IEntityTypeConfiguration<ProjectRanking>
{
    public void Configure(EntityTypeBuilder<ProjectRanking> builder)
    {
        builder.ToTable("ProjectRankings");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rank).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasOne(r => r.StudentUser)
            .WithMany()
            .HasForeignKey(r => r.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Project)
            .WithMany()
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // One ranking entry per student–project pair.
        builder.HasIndex(r => new { r.StudentUserId, r.ProjectId }).IsUnique();

        // Each rank slot (1–3) used at most once per student.
        builder.HasIndex(r => new { r.StudentUserId, r.Rank }).IsUnique();

        // Matching / admin: list all students who ranked a project.
        builder.HasIndex(r => r.ProjectId);
        builder.HasIndex(r => new { r.ProjectId, r.Rank });

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_ProjectRankings_Rank",
                $"[Rank] >= {ProjectRanking.MinRank} AND [Rank] <= {ProjectRanking.MaxRank}");
        });
    }
}
