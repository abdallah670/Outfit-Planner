using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Configurations;

public class SentReminderConfiguration : IEntityTypeConfiguration<SentReminder>
{
    public void Configure(EntityTypeBuilder<SentReminder> builder)
    {
        builder.ToTable("SentReminders");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.ReminderType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.SentAt)
            .IsRequired();

        builder.HasIndex(e => new { e.UserId, e.CalendarEventId, e.ReminderType, e.SentAt });
        builder.HasIndex(e => e.UserId);
    }
}
