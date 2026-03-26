using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class OutfitCommentConfiguration : IEntityTypeConfiguration<OutfitComment>
{
    public void Configure(EntityTypeBuilder<OutfitComment> builder)
    {
        builder.ToTable("OutfitComments");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(2000);
            
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
            
        builder.HasOne(x => x.ParentComment)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction); // Protect against cycle cascades
            
        builder.HasIndex(x => x.OutfitId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ParentCommentId);
    }
}
