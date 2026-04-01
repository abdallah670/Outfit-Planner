using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class VoteCommentConfiguration : IEntityTypeConfiguration<VoteComment>
{
    public void Configure(EntityTypeBuilder<VoteComment> builder)
    {
        builder.HasKey(vc => vc.Id);
        
        builder.Property(vc => vc.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasOne(vc => vc.Vote)
            .WithMany()
            .HasForeignKey(vc => vc.VoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vc => vc.User)
            .WithMany()
            .HasForeignKey(vc => vc.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(vc => vc.ParentComment)
            .WithMany(pc => pc.Replies)
            .HasForeignKey(vc => vc.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
