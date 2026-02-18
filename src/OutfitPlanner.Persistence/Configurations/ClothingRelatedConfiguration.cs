using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class ClothingTagConfiguration : IEntityTypeConfiguration<ClothingTag>
{
    public void Configure(EntityTypeBuilder<ClothingTag> builder)
    {
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Source)
            .HasMaxLength(50);
            
        builder.Property(t => t.Confidence)
            .HasPrecision(5, 4);
    }
}

public class OutfitFeedbackConfiguration : IEntityTypeConfiguration<OutfitFeedback>
{
    public void Configure(EntityTypeBuilder<OutfitFeedback> builder)
    {
        builder.Property(f => f.Comment)
            .HasMaxLength(1000);

        builder.HasOne(f => f.Reviewer)
            .WithMany()
            .HasForeignKey(f => f.ReviewerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class OutfitItemConfiguration : IEntityTypeConfiguration<OutfitItem>
{
    public void Configure(EntityTypeBuilder<OutfitItem> builder)
    {
        builder.Property(i => i.Role)
            .HasConversion<string>();

        builder.HasOne(i => i.ClothingItem)
            .WithMany(c => c.OutfitItems)
            .HasForeignKey(i => i.ClothingItemId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
