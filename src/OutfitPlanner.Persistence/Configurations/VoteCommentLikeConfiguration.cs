using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class VoteCommentLikeConfiguration : IEntityTypeConfiguration<VoteCommentLike>
{
    public void Configure(EntityTypeBuilder<VoteCommentLike> builder)
    {
        builder.HasKey(vl => vl.Id);

        builder.HasIndex(vl => new { vl.CommentId, vl.UserId }).IsUnique();

        builder.HasOne(vl => vl.Comment)
            .WithMany(vc => vc.Likes)
            .HasForeignKey(vl => vl.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vl => vl.User)
            .WithMany()
            .HasForeignKey(vl => vl.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
