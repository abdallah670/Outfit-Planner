using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Persistence;

namespace OutfitPlanner.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IClothingItemRepository ClothingItems { get; }
    public IOutfitRepository Outfits { get; }
    public IValidationPollRepository ValidationPolls { get; }
    public IWearEventRepository WearEvents { get; }
    public IUserStyleProfileRepository UserStyleProfiles { get; }

    public IUserRepository Users { get; }
    public IStyleRuleRepository StyleRules { get; }
    public IClothingTagRepository ClothingTags { get; }
    public IOutfitItemRepository OutfitItems { get; }
    public IPollOptionRepository PollOptions { get; }
    public IVoteRepository Votes { get; }
    public IUserPreferencesRepository UserPreferences { get; }
    public ICalendarEventRepository CalendarEvents { get; }
    public INotificationRepository Notifications { get; }
    public IAppPreferencesRepository AppPreferences { get; }
    public INotificationSettingsRepository NotificationSettings { get; }
    public IFeedPostRepository FeedPosts { get; }
    public IPostReactionRepository PostReactions { get; }
    public IPostCommentRepository PostComments { get; }
    public IFollowRepository Follows { get; }
    public IAuditLogRepository AuditLogs { get; }
    public ISystemSettingRepository SystemSettings { get; }
    public IContentReportRepository ContentReports { get; }
   

    public UnitOfWork(
        AppDbContext context,
        IClothingItemRepository clothingItems,
        IOutfitRepository outfits,
        IValidationPollRepository validationPolls,
        IWearEventRepository wearEvents,
        IUserStyleProfileRepository userStyleProfiles,
        IUserRepository users,
        IStyleRuleRepository styleRules,
        IClothingTagRepository clothingTags,
        IOutfitItemRepository outfitItems,
        IPollOptionRepository pollOptions,
        IVoteRepository votes,
        IUserPreferencesRepository userPreferences,
        ICalendarEventRepository calendarEvents,
        INotificationRepository notifications,
        IAppPreferencesRepository appPreferences,
        INotificationSettingsRepository notificationSettings,
        IFeedPostRepository feedPosts,
        IPostReactionRepository postReactions,
        IPostCommentRepository postComments,
        IFollowRepository follows,
        IAuditLogRepository auditLogs,
        ISystemSettingRepository systemSettings,
        IContentReportRepository contentReports
       )
    {
        _context = context;
        ClothingItems = clothingItems;
        Outfits = outfits;
        ValidationPolls = validationPolls;
        WearEvents = wearEvents;
        UserStyleProfiles = userStyleProfiles;

        Users = users;
        StyleRules = styleRules;
        ClothingTags = clothingTags;
        OutfitItems = outfitItems;
        PollOptions = pollOptions;
        Votes = votes;
        UserPreferences = userPreferences;
        CalendarEvents = calendarEvents;
        Notifications = notifications;
        AppPreferences = appPreferences;
        NotificationSettings = notificationSettings;
        FeedPosts = feedPosts;
        PostReactions = postReactions;
        PostComments = postComments;
        Follows = follows;
        AuditLogs = auditLogs;
        SystemSettings = systemSettings;
        ContentReports = contentReports;
    }


    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        return new GenericRepository<TEntity>(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
