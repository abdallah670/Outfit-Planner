using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class OutfitConfiguration : IEntityTypeConfiguration<Outfit>
{
    public void Configure(EntityTypeBuilder<Outfit> builder)
    {
        builder.Property(o => o.Name).HasMaxLength(200);
        
        builder.Property(o => o.Occasion)
            .HasConversion<string>(); // Store enum as string

        builder.Property(o => o.Status)
            .HasConversion<string>();

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Outfit)
            .HasForeignKey(i => i.OutfitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Feedback)
            .WithOne(f => f.Outfit)
            .HasForeignKey(f => f.OutfitId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
