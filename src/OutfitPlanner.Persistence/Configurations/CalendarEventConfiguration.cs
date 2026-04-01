using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for CalendarEvent entity
/// </summary>
public class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.ToTable("CalendarEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Location)
            .HasMaxLength(200);

        builder.Property(e => e.EventDate)
            .IsRequired();

        builder.Property(e => e.StartTime);

        builder.Property(e => e.EndTime);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.IsRecurring)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.RecurrencePattern)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.WearEvent)
            .WithMany()
            .HasForeignKey(e => e.WearEventId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.EventDate);
        builder.HasIndex(e => new { e.UserId, e.EventDate });
    }
}
