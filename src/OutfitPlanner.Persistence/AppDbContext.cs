using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ClothingItem> ClothingItems { get; set; }
    public DbSet<ClothingTag> ClothingTags { get; set; }
    public DbSet<Outfit> Outfits { get; set; }
    public DbSet<OutfitItem> OutfitItems { get; set; }
    public DbSet<ValidationPoll> ValidationPolls { get; set; }
    public DbSet<PollOption> PollOptions { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<TrendingOutfit> TrendingOutfits { get; set; }
    public DbSet<WearEvent> WearEvents { get; set; }
    public DbSet<CalendarEvent> CalendarEvents { get; set; }
    public DbSet<UserStyleProfile> UserStyleProfiles { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }
    public DbSet<StyleRule> StyleRules { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AppPreferences> AppPreferences { get; set; }
    public DbSet<NotificationSettings> NotificationSettings { get; set; }
    public DbSet<FeedPost> FeedPosts { get; set; }
    public DbSet<PostReaction> PostReactions { get; set; }
    public DbSet<PostComment> PostComments { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<UserActivity> UserActivities { get; set; }
    
    // Admin entities
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<ContentReport> ContentReports { get; set; }
    
    // AI Chat entities
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<SentReminder> SentReminders { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply configurations from assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Apply global query filter for soft delete on all BaseEntity subtypes
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    ?.MakeGenericMethod(entityType.ClrType);
                method?.Invoke(null, new object[] { builder });
            }
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : BaseEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    /// <summary>
    /// Soft deletes an entity by setting IsDeleted = true and DeletedAt = UtcNow.
    /// Override SaveChangesAsync to auto-set UpdatedAt on all tracked entities.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
            }
        }


        return await base.SaveChangesAsync(cancellationToken);
    }
}