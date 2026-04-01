using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class AppPreferencesConfiguration : IEntityTypeConfiguration<AppPreferences>
{
    public void Configure(EntityTypeBuilder<AppPreferences> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.TemperatureUnit)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.Language)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(p => p.Theme)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.MeasurementUnit)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.HasIndex(p => p.UserId)
            .IsUnique();
    }
}
