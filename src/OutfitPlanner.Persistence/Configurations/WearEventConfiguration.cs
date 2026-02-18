using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class WearEventConfiguration : IEntityTypeConfiguration<WearEvent>
{
    public void Configure(EntityTypeBuilder<WearEvent> builder)
    {
        builder.Property(e => e.WeatherCondition)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.HasOne(e => e.ClothingItem)
            .WithMany(i => i.WearEvents)
            .HasForeignKey(e => e.ClothingItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Outfit)
            .WithMany()
            .HasForeignKey(e => e.OutfitId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasOne(e => e.User)
            .WithMany(u => u.WearEvents)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
