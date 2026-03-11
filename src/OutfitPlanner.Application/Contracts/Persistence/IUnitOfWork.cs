namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IUnitOfWork : IDisposable
{
    IClothingItemRepository ClothingItems { get; }
    IOutfitRepository Outfits { get; }
    IValidationPollRepository ValidationPolls { get; }
    IWearEventRepository WearEvents { get; }
    IUserStyleProfileRepository UserStyleProfiles { get; }
    IOutfitFeedbackRepository OutfitFeedbacks { get; }
    IUserRepository Users { get; }
    IStyleRuleRepository StyleRules { get; }
    IClothingTagRepository ClothingTags { get; }
    IOutfitItemRepository OutfitItems { get; }
    IPollOptionRepository PollOptions { get; }
    IVoteRepository Votes { get; }
    IUserPreferencesRepository UserPreferences { get; }
    ICalendarEventRepository CalendarEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
