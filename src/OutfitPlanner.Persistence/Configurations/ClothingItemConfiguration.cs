using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.ValueObjects;

namespace OutfitPlanner.Persistence.Configurations;

public class ClothingItemConfiguration : IEntityTypeConfiguration<ClothingItem>
{
    public void Configure(EntityTypeBuilder<ClothingItem> builder)
    {
        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Value Object: PurchasePrice (Money)
        builder.OwnsOne(i => i.PurchasePrice, priceBuilder =>
        {
            priceBuilder.Property(p => p.Amount).HasColumnName("PurchasePrice").HasColumnType("decimal(18,2)");
            priceBuilder.Property(p => p.Currency).HasColumnName("PurchaseCurrency").HasMaxLength(3);
        });

        // FabricType is Enum, stored as int by default or string?
        builder.Property(i => i.Fabric)
            .HasConversion<string>(); // Store enum as string for readability

        builder.Property(i => i.Type).HasConversion<string>();
        
        builder.HasMany(i => i.Tags)
            .WithOne(t => t.ClothingItem)
            .HasForeignKey(t => t.ClothingItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
