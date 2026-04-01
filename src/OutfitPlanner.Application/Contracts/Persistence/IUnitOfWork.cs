using OutfitPlanner.Application.Contracts.Persistence;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IUnitOfWork : IDisposable
{
    IClothingItemRepository ClothingItems { get; }
    IOutfitRepository Outfits { get; }
    IValidationPollRepository ValidationPolls { get; }
    IWearEventRepository WearEvents { get; }
    IUserStyleProfileRepository UserStyleProfiles { get; }
    
    IUserRepository Users { get; }
    IStyleRuleRepository StyleRules { get; }
    IClothingTagRepository ClothingTags { get; }
    IOutfitItemRepository OutfitItems { get; }
    IPollOptionRepository PollOptions { get; }
    IVoteRepository Votes { get; }
    IUserPreferencesRepository UserPreferences { get; }
    ICalendarEventRepository CalendarEvents { get; }
    INotificationRepository Notifications { get; }
    IAppPreferencesRepository AppPreferences { get; }
    INotificationSettingsRepository NotificationSettings { get; }
    IFeedPostRepository FeedPosts { get; }
    IPostReactionRepository PostReactions { get; }
    IPostCommentRepository PostComments { get; }
    IFollowRepository Follows { get; }
   

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
