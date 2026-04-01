using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Identity User configuration if needed, usually defaults are fine.
        // Configure relationships
        builder.HasOne(u => u.StyleProfile)
            .WithOne() // UserStyleProfile has UserId, but navigation back to User? In UserStyleProfile we commented out User nav prop. 
            .HasForeignKey<UserStyleProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Preferences)
            .WithOne()
            .HasForeignKey<UserPreferences>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ClothingItems)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Outfits)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Deleting user deletes outfits

        builder.HasMany(u => u.Polls)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(u => u.WearEvents)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent cycles or cascading issues if needed, or Cascade? 
            // If User is deleted, WearEvents should probably be deleted too: Cascade.
            // But sometimes EF Core complains about multiple cascade paths. 
            // ClothingItem -> WearEvent (Cascade)
            // Outfit -> WearEvent (Cascade ?)
            // User -> WearEvent (Cascade)
            // This creates multiple cascade paths. Let's start with Cascade and see if migration fails, or restrict one.
            // Restricting User->WearEvent seems safer if Item/Outfit deletion already handles it?
            // Actually, if User is deleted, EVERYTHING is deleted. That's fine.
            // But if Item is deleted, specific wear events go.
            // Let's create configurations for those specifically.
    }
}
