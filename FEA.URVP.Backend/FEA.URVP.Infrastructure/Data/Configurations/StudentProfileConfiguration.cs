using System.Text.Json;
using FEA.URVP.Domain.Entities.StudentProfiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.ToTable("StudentProfiles");

        builder.HasKey(p => p.Id);

        builder.HasIndex(p => p.UserId).IsUnique();

        builder.Property(p => p.Gender)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(p => p.MobileNumber)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(p => p.Degree)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(p => p.ExpectedGraduationYear)
            .IsRequired();

        ConfigureStringList(builder.Property(p => p.Languages), "nvarchar(2000)");

        builder.Property(p => p.OtherLanguages)
            .HasMaxLength(256);

        builder.Property(p => p.CompletedCredits)
            .IsRequired();

        builder.Property(p => p.CumulativeAverage)
            .IsRequired()
            .HasPrecision(5, 2);

        ConfigureStringList(builder.Property(p => p.ResearchTopics), "nvarchar(2000)");

        builder.Property(p => p.Publications)
            .HasMaxLength(4000);

        builder.Property(p => p.TranscriptFileId);
        builder.Property(p => p.CitiFileId);

        builder.Property(p => p.Availability)
            .IsRequired()
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<DayAvailability>>(v, JsonOptions)
                    ?? new List<DayAvailability>())
            .Metadata.SetValueComparer(
                new ValueComparer<List<DayAvailability>>(
                    (a, b) => JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
                    v => JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),
                    v => JsonSerializer.Deserialize<List<DayAvailability>>(
                        JsonSerializer.Serialize(v, JsonOptions), JsonOptions)
                        ?? new List<DayAvailability>()));

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureStringList(
        PropertyBuilder<List<string>> property,
        string columnType)
    {
        property
            .IsRequired()
            .HasColumnType(columnType)
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
