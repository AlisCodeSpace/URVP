using System.Text.Json;
using FEA.URVP.Domain.Entities.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        ConfigureStringList(builder.Property(p => p.ResearchAreas));
        ConfigureStringList(builder.Property(p => p.ActivityTypes));

        builder.Property(p => p.IrbStage)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(p => p.BriefDescription)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(p => p.VolunteersRequired)
            .IsRequired();

        builder.Property(p => p.VolunteersFilled)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.MinQualifications)
            .HasMaxLength(2000);

        builder.Property(p => p.AdditionalComments)
            .HasMaxLength(2000);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(p => p.FacultyNameSnapshot)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(p => p.AffiliationSnapshot)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.EmailSnapshot)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.UserNameSnapshot)
            .HasMaxLength(64);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.CreatedByUserId);
        builder.HasIndex(p => new { p.Status, p.CreatedAt });

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Projects_VolunteersRequired", "[VolunteersRequired] >= 1");
            t.HasCheckConstraint("CK_Projects_VolunteersFilled", "[VolunteersFilled] >= 0");
        });
    }

    private static void ConfigureStringList(
        Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<List<string>> property)
    {
        property
            .IsRequired()
            .HasColumnType("nvarchar(2000)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .Metadata.SetValueComparer(
                new ValueComparer<List<string>>(
                    (a, b) => a!.SequenceEqual(b!),
                    v => v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                    v => v.ToList()));
    }
}
