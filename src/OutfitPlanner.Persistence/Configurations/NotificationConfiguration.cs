using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        
        builder.HasKey(x => x.Id);
        
        // Configure NotificationType enum to be stored as string
        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.Property(x => x.ActionUrl)
            .HasMaxLength(500);
        
        builder.Property(x => x.UserId)
            .IsRequired();
        
        builder.Property(x => x.IsRead)
            .IsRequired();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IsRead);
        builder.HasIndex(x => x.CreatedAt);
    }
}
