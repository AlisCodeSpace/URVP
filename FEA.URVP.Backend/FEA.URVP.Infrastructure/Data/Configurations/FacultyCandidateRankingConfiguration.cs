using FEA.URVP.Domain.Entities.FacultyCandidateRankings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class FacultyCandidateRankingConfiguration : IEntityTypeConfiguration<FacultyCandidateRanking>
{
    public void Configure(EntityTypeBuilder<FacultyCandidateRanking> builder)
    {
        builder.ToTable("FacultyCandidateRankings");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rank).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasOne(r => r.Project)
            .WithMany()
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.StudentUser)
            .WithMany()
            .HasForeignKey(r => r.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One faculty ranking per student–project pair.
        builder.HasIndex(r => new { r.ProjectId, r.StudentUserId }).IsUnique();

        builder.HasIndex(r => new { r.ProjectId, r.Rank });

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_FacultyCandidateRankings_Rank",
                $"[Rank] >= {FacultyCandidateRanking.MinRank}");
        });
    }
}
