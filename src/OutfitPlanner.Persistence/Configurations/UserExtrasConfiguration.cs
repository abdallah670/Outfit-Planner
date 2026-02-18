using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class UserStyleProfileConfiguration : IEntityTypeConfiguration<UserStyleProfile>
{
    public void Configure(EntityTypeBuilder<UserStyleProfile> builder)
    {
        builder.Property(p => p.Style)
            .HasConversion<string>();

        builder.Property(p => p.FitPreferences)
            .HasMaxLength(500);

        builder.HasMany(p => p.CustomRules)
            .WithOne()
            .HasForeignKey(r => r.UserStyleProfileId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Note: PreferredColors is List<string>, EF Core 9+ supports primitive collections naturally.
        // For older or specific behavior, we might need a converter, but net9.0 usually handles it.
    }
}

public class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.Property(p => p.DefaultOutfitPrivacy)
            .HasConversion<string>();
    }
}

public class StyleRuleConfiguration : IEntityTypeConfiguration<StyleRule>
{
    public void Configure(EntityTypeBuilder<StyleRule> builder)
    {
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);
    }
}
