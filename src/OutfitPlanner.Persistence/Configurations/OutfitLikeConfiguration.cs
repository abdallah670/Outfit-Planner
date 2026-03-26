using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class OutfitLikeConfiguration : IEntityTypeConfiguration<OutfitLike>
{
    public void Configure(EntityTypeBuilder<OutfitLike> builder)
    {
        builder.ToTable("OutfitLikes");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
            
        builder.HasOne(x => x.Outfit)
            .WithMany()
            .HasForeignKey(x => x.OutfitId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
            
        // Prevent duplicate likes: one like per user per outfit
        builder.HasIndex(x => new { x.OutfitId, x.UserId })
            .IsUnique();
            
        builder.HasIndex(x => x.UserId);
    }
}
