using System.Text.Json;
using FEA.URVP.Domain.Entities.Matching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class MatchingRunConfiguration : IEntityTypeConfiguration<MatchingRun>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public void Configure(EntityTypeBuilder<MatchingRun> builder)
    {
        builder.ToTable("MatchingRuns");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(r => r.AlgorithmVersion)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(r => r.Seed).IsRequired();
        builder.Property(r => r.StudentsConsidered).IsRequired();
        builder.Property(r => r.ProjectsConsidered).IsRequired();
        builder.Property(r => r.SeatsAvailable).IsRequired();
        builder.Property(r => r.StudentsMatched).IsRequired();
        builder.Property(r => r.TieBreaksUsed).IsRequired();

        builder.Property(r => r.Warnings)
            .IsRequired()
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(
                new ValueComparer<List<string>>(
                    (a, b) => a!.SequenceEqual(b!),
                    v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                    v => v.ToList()));

        builder.Property(r => r.CreatedByUserId).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasOne(r => r.Semester)
            .WithMany()
            .HasForeignKey(r => r.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Placements)
            .WithOne(p => p.MatchingRun)
            .HasForeignKey(p => p.MatchingRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.SemesterId, r.Status });
    }
}
