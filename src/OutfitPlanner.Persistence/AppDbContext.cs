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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply configurations from assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Customize conventions if needed (e.g. usage of Value Objects to be configured via Config classes)
    }
}
