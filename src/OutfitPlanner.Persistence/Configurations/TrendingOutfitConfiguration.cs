using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class TrendingOutfitConfiguration : IEntityTypeConfiguration<TrendingOutfit>
{
    public void Configure(EntityTypeBuilder<TrendingOutfit> builder)
    {
        builder.ToTable("TrendingOutfits");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.TrendingScore)
            .HasPrecision(10, 2)
            .IsRequired();
        
        builder.Property(x => x.VoteCount)
            .IsRequired();
        
        builder.Property(x => x.LikeCount)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(x => x.CommentCount)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(x => x.ReactionCount)
            .IsRequired();
        
        builder.Property(x => x.RankPosition)
            .IsRequired();
        
        builder.Property(x => x.Date)
            .IsRequired();
        
        builder.HasOne(x => x.Outfit)
            .WithMany()
            .HasForeignKey(x => x.OutfitId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.Poll)
            .WithMany()
            .HasForeignKey(x => x.PollId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
        
        builder.HasIndex(x => new { x.OutfitId, x.Date })
            .IsUnique();
        
        builder.HasIndex(x => new { x.Date, x.TrendingScore });
        builder.HasIndex(x => new { x.Date, x.RankPosition });
    }
}
