using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.SessionId)
            .IsRequired();
        
        builder.Property(x => x.SenderId)
            .IsRequired()
            .HasMaxLength(450);
        
        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(10000);
        
        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(x => x.Intent)
            .HasMaxLength(50)
            .IsRequired(false);
        
        builder.Property(x => x.Metadata)
            .IsRequired(false);
        
        builder.HasOne(x => x.Session)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.CreatedAt);
    }
}