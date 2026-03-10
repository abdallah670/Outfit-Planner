using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class VoteReactionConfiguration : IEntityTypeConfiguration<VoteReaction>
{
    public void Configure(EntityTypeBuilder<VoteReaction> builder)
    {
        builder.ToTable("VoteReactions");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ReactionType)
            .IsRequired();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasOne(x => x.Vote)
            .WithMany(v => v.Reactions)
            .HasForeignKey(x => x.VoteId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasIndex(x => new { x.VoteId, x.UserId })
            .IsUnique();
    }
}
