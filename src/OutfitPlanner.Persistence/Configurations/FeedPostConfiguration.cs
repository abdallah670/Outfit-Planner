using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class FeedPostConfiguration : IEntityTypeConfiguration<FeedPost>
{
    public void Configure(EntityTypeBuilder<FeedPost> builder)
    {
        builder.ToTable("FeedPosts");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.UserId)
            .IsRequired();
            
        builder.Property(x => x.PostType)
            .IsRequired();
            
        builder.Property(x => x.Caption)
            .HasMaxLength(500);
            
        builder.Property(x => x.Visibility)
            .HasConversion<int>();
            
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasOne(x => x.Outfit)
            .WithMany()
            .HasForeignKey(x => x.OutfitId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasOne(x => x.Poll)
            .WithMany()
            .HasForeignKey(x => x.PollId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.LikeCount);
    }
}

public class PostReactionConfiguration : IEntityTypeConfiguration<PostReaction>
{
    public void Configure(EntityTypeBuilder<PostReaction> builder)
    {
        builder.ToTable("PostReactions");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.UserId)
            .IsRequired();
            
        builder.Property(x => x.ReactionType)
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(x => x.CreatedAt)
            .IsRequired();
            
        builder.HasOne(x => x.Post)
            .WithMany(p => p.Reactions)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
            
        // Prevent duplicate reactions: one reaction per user per post
        builder.HasIndex(x => new { x.PostId, x.UserId })
            .IsUnique();
            
        builder.HasIndex(x => x.UserId);
    }
}

public class PostCommentConfiguration : IEntityTypeConfiguration<PostComment>
{
    public void Configure(EntityTypeBuilder<PostComment> builder)
    {
        builder.ToTable("PostComments");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.UserId)
            .IsRequired();
            
        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);
            
        builder.HasOne(x => x.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasOne(x => x.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasIndex(x => x.PostId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ParentCommentId);
    }
}

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.ToTable("Follows");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.FollowerId)
            .IsRequired();
            
        builder.Property(x => x.FollowingId)
            .IsRequired();
            
        builder.Property(x => x.CreatedAt)
            .IsRequired();
            
        builder.HasOne(x => x.Follower)
            .WithMany()
            .HasForeignKey(x => x.FollowerId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasOne(x => x.Following)
            .WithMany()
            .HasForeignKey(x => x.FollowingId)
            .OnDelete(DeleteBehavior.NoAction);
            
        // Prevent duplicate follows
        builder.HasIndex(x => new { x.FollowerId, x.FollowingId })
            .IsUnique();
            
        builder.HasIndex(x => x.FollowerId);
        builder.HasIndex(x => x.FollowingId);
    }
}
