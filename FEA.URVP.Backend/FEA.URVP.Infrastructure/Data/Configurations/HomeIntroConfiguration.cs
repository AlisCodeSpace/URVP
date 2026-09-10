using System.Text.Json;
using FEA.URVP.Domain.Entities.HomeIntro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FEA.URVP.Infrastructure.Data.Configurations;

public sealed class HomeIntroConfiguration : IEntityTypeConfiguration<HomeIntro>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public void Configure(EntityTypeBuilder<HomeIntro> builder)
    {
        builder.ToTable("HomeIntro");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Headline)
            .IsRequired()
            .HasMaxLength(HomeIntro.HeadlineMaxLength);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(HomeIntro.DescriptionMaxLength);

        builder.Property(x => x.KeyPoints)
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

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.UpdatedByUserId);
    }
}
