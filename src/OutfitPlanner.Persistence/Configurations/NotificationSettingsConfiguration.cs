using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class NotificationSettingsConfiguration : IEntityTypeConfiguration<NotificationSettings>
{
    public void Configure(EntityTypeBuilder<NotificationSettings> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.DailyOutfitSuggestion)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.WeeklyStyleReport)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.WeatherAlerts)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.NewFeatures)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.SocialNotifications)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.PushNotificationsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.HasIndex(p => p.UserId)
            .IsUnique();
    }
}
