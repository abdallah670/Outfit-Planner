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
    public IOutfitFeedbackRepository OutfitFeedbacks { get; }
    public IUserRepository Users { get; }
    public IStyleRuleRepository StyleRules { get; }
    public IClothingTagRepository ClothingTags { get; }
    public IOutfitItemRepository OutfitItems { get; }
    public IPollOptionRepository PollOptions { get; }
    public IVoteRepository Votes { get; }
    public IUserPreferencesRepository UserPreferences { get; }
    public ICalendarEventRepository CalendarEvents { get; }
    public INotificationRepository Notifications { get; }

    public UnitOfWork(
        AppDbContext context,
        IClothingItemRepository clothingItems,
        IOutfitRepository outfits,
        IValidationPollRepository validationPolls,
        IWearEventRepository wearEvents,
        IUserStyleProfileRepository userStyleProfiles,
        IOutfitFeedbackRepository outfitFeedbacks,
        IUserRepository users,
        IStyleRuleRepository styleRules,
        IClothingTagRepository clothingTags,
        IOutfitItemRepository outfitItems,
        IPollOptionRepository pollOptions,
        IVoteRepository votes,
        IUserPreferencesRepository userPreferences,
        ICalendarEventRepository calendarEvents,
        INotificationRepository notifications)
    {
        _context = context;
        ClothingItems = clothingItems;
        Outfits = outfits;
        ValidationPolls = validationPolls;
        WearEvents = wearEvents;
        UserStyleProfiles = userStyleProfiles;
        OutfitFeedbacks = outfitFeedbacks;
        Users = users;
        StyleRules = styleRules;
        ClothingTags = clothingTags;
        OutfitItems = outfitItems;
        PollOptions = pollOptions;
        Votes = votes;
        UserPreferences = userPreferences;
        CalendarEvents = calendarEvents;
        Notifications = notifications;
    }
    

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
