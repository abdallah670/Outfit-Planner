using AutoMapper;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.DTOs.User;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Domain.ValueObjects;

namespace OutfitPlanner.Application.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // =====================
        // VALUE OBJECT MAPPINGS
        // =====================
        CreateMap<Money, decimal>().ConvertUsing(src => src.Amount);
        CreateMap<decimal, Money>().ConvertUsing(src => Money.From(src, "USD"));

        // ====================
        // WARDROBE MAPPINGS
        // ====================
        
        // ClothingItem - Create
        CreateMap<CreateClothingItemDto, ClothingItem>()
            .ForMember(d => d.PurchasePrice, opt => opt.MapFrom(s => Money.From(s.PurchasePrice, s.Currency)))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => Enum.Parse<ClothingType>(s.Type, true)))
            .ForMember(d => d.Fabric, opt => opt.MapFrom(s => Enum.Parse<FabricType>(s.Fabric, true)));

        // ClothingItem - Update
        CreateMap<UpdateClothingItemDto, ClothingItem>()
            .ForMember(d => d.PurchasePrice, opt => opt.MapFrom(s => Money.From(s.PurchasePrice, s.Currency ?? "USD")))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => Enum.Parse<ClothingType>(s.Type, true)));

        // ClothingItem - To DTOs
        CreateMap<ClothingItem, ClothingItemDto>()
            .ForMember(d => d.PurchasePrice, opt => opt.MapFrom(s => s.PurchasePrice.Amount))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.PurchasePrice.Currency))
            .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Condition, opt => opt.MapFrom(s => s.Condition.ToString()))
            .ReverseMap();

        CreateMap<ClothingItem, ClothingItemListDto>()
            .ForMember(d => d.PurchasePrice, opt => opt.MapFrom(s => s.PurchasePrice.Amount))
            .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()));

        // WearEvent mappings
        CreateMap<RecordWearDto, WearEvent>()
            .ForMember(d => d.ClothingItemId, opt => opt.MapFrom(s => s.ClothingItemId))
            .ForMember(d => d.WornAt, opt => opt.MapFrom(s => s.WornAt))
            .ForMember(d => d.DurationMinutes, opt => opt.MapFrom(s => s.DurationMinutes ?? 0))
            .ForMember(d => d.WeatherCondition, opt => opt.MapFrom(s => s.WeatherCondition ?? string.Empty))
            .ForMember(d => d.Rating, opt => opt.MapFrom(s => s.Rating ?? 0))
            .ForMember(d => d.Notes, opt => opt.MapFrom(s => s.Notes ?? string.Empty));

        // ====================
        // OUTFIT MAPPINGS
        // ====================
        
        // Outfit - Create
        CreateMap<CreateOutfitDto, Outfit>()
            .ForMember(d => d.Occasion, opt => opt.MapFrom(s => Enum.Parse<OccasionType>(s.Occasion, true)))
            .ForMember(d => d.Season, opt => opt.MapFrom(s => Enum.Parse<Season>(s.Season, true)))
            
            .ForMember(d => d.ImageUrl, opt => opt.Ignore());

        // Outfit - Update
        CreateMap<UpdateOutfitDto, Outfit>()
            .ForMember(d => d.Occasion, opt => opt.MapFrom(s => s.Occasion != null ? Enum.Parse<OccasionType>(s.Occasion, true) : default(OccasionType)))
            .ForMember(d => d.Season, opt => opt.MapFrom(s => s.Season != null ? Enum.Parse<Season>(s.Season, true) : default(Season)))
            .ForMember(d => d.UserId, opt => opt.Ignore())
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.TimesWorn, opt => opt.Ignore())
            .ForMember(d => d.LastWorn, opt => opt.Ignore())
            .ForMember(d => d.ComfortRating, opt => opt.Condition(s => s.ComfortRating.HasValue))
            .ForMember(d => d.Name, opt => opt.Condition(s => !string.IsNullOrEmpty(s.Name)))
            .ForMember(d => d.WeatherCondition, opt => opt.Condition(s => !string.IsNullOrEmpty(s.WeatherCondition)))
            .ForMember(d => d.ImageUrl, opt => opt.Condition(s => !string.IsNullOrEmpty(s.ImageUrl)))
            .ForMember(d => d.Items, opt => opt.Ignore());

        // Outfit - To DTOs
        CreateMap<Outfit, OutfitDto>()
            .ForMember(d => d.Occasion, opt => opt.MapFrom(s => s.Occasion.ToString()))
            .ForMember(d => d.Season, opt => opt.MapFrom(s => s.Season.ToString()));

        CreateMap<OutfitDto, Outfit>()
            .ForMember(d => d.Occasion, opt => opt.MapFrom(s => Enum.Parse<OccasionType>(s.Occasion, true)))
            .ForMember(d => d.Season, opt => opt.MapFrom(s => Enum.Parse<Season>(s.Season, true)));

        CreateMap<Outfit, OutfitListDto>()
            .ForMember(d => d.Occasion, opt => opt.MapFrom(s => s.Occasion.ToString()))
            .ForMember(d => d.Season, opt => opt.MapFrom(s => s.Season.ToString()))
            .ForMember(d => d.ItemCount, opt => opt.MapFrom(s => s.Items.Count))
            .ForMember(d => d.ThumbnailUrl, opt => opt.MapFrom(s => s.Items.FirstOrDefault() != null ? s.Items.FirstOrDefault().ClothingItem.ThumbnailUrl : string.Empty));

        // OutfitItem mappings
        CreateMap<OutfitItem, OutfitItemDto>()
            .ForMember(d => d.ClothingItemName, opt => opt.MapFrom(s => s.ClothingItem.Name))
            .ForMember(d => d.ClothingItemImageUrl, opt => opt.MapFrom(s => s.ClothingItem.ImageUrl))
            .ForMember(d => d.ClothingItemType, opt => opt.MapFrom(s => s.ClothingItem.Type))
            .ForMember(d => d.ClothingItemCategory, opt => opt.MapFrom(s => s.ClothingItem.Category.ToString()))
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()));

        CreateMap<CreateOutfitItemDto, OutfitItem>();

        // ====================
        // USER MAPPINGS
        // ====================
        
        // User - To Profile DTO
        CreateMap<User, UserProfileDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email ?? string.Empty))
            .ForMember(d => d.StyleProfile, opt => opt.MapFrom(s => s.StyleProfile))
            .ForMember(d => d.Preferences, opt => opt.MapFrom(s => s.Preferences));

        // StyleRule mappings
        CreateMap<StyleRule, StyleRuleDto>();
        CreateMap<CreateStyleRuleDto, StyleRule>();
        CreateMap<UpdateStyleRuleDto, StyleRule>();

        // UserStyleProfile mappings
        CreateMap<UserStyleProfile, UserStyleProfileDto>()
            .ForMember(d => d.Style, opt => opt.MapFrom(s => s.Style.ToString()))
            .ForMember(d => d.PreferredColors, opt => opt.MapFrom(s => s.PreferredColors))
            .ForMember(d => d.FitPreferences, opt => opt.MapFrom(s => s.FitPreferences))
            .ForMember(d => d.ComfortPriority, opt => opt.MapFrom(s => s.ComfortPriority))
            .ForMember(d => d.AcceptsTrends, opt => opt.MapFrom(s => s.AcceptsTrends))
            .ForMember(d => d.CustomRules, opt => opt.MapFrom(s => s.CustomRules));

        // UserPreferences mappings
        CreateMap<UserPreferences, UserPreferencesDto>()
            .ForMember(d => d.ShareOutfitsAnonymously, opt => opt.MapFrom(s => s.ShareOutfitsAnonymously))
            .ForMember(d => d.IncludeInTrendAnalysis, opt => opt.MapFrom(s => s.IncludeInTrendAnalysis))
            .ForMember(d => d.AllowFriendRequests, opt => opt.MapFrom(s => s.AllowFriendRequests))
            .ForMember(d => d.DefaultOutfitPrivacy, opt => opt.MapFrom(s => s.DefaultOutfitPrivacy.ToString()))
            .ForMember(d => d.ShowBodyMetrics, opt => opt.MapFrom(s => s.ShowBodyMetrics))
            .ForMember(d => d.AllowLocationTracking, opt => opt.MapFrom(s => s.AllowLocationTracking));

        // Update DTOs to Entities
        CreateMap<UpdateUserProfileDto, User>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.ProfilePictureUrl, opt => opt.Ignore());

        CreateMap<UpdateStyleProfileDto, UserStyleProfile>()
            .ForMember(d => d.Style, opt => opt.MapFrom(s => s.Style))
            .ForMember(d => d.PreferredColors, opt => opt.MapFrom(s => s.PreferredColors))
            .ForMember(d => d.FitPreferences, opt => opt.MapFrom(s => s.FitPreferences))
            .ForMember(d => d.ComfortPriority, opt => opt.MapFrom(s => s.ComfortPriority))
            .ForMember(d => d.AcceptsTrends, opt => opt.MapFrom(s => s.AcceptsTrends));

        CreateMap<UpdateUserPreferencesDto, UserPreferences>()
            .ForMember(d => d.ShareOutfitsAnonymously, opt => opt.MapFrom(s => s.ShareOutfitsAnonymously))
            .ForMember(d => d.IncludeInTrendAnalysis, opt => opt.MapFrom(s => s.IncludeInTrendAnalysis))
            .ForMember(d => d.AllowFriendRequests, opt => opt.MapFrom(s => s.AllowFriendRequests))
            .ForMember(d => d.DefaultOutfitPrivacy, opt => opt.MapFrom(s => s.DefaultOutfitPrivacy))
            .ForMember(d => d.ShowBodyMetrics, opt => opt.MapFrom(s => s.ShowBodyMetrics))
            .ForMember(d => d.AllowLocationTracking, opt => opt.MapFrom(s => s.AllowLocationTracking));

        // ====================

        // UserPreferences mappings
        CreateMap<UserPreferences, UserPreferencesDto>()
            .ForMember(d => d.ShareOutfitsAnonymously, opt => opt.MapFrom(s => s.ShareOutfitsAnonymously))
            .ForMember(d => d.IncludeInTrendAnalysis, opt => opt.MapFrom(s => s.IncludeInTrendAnalysis))
            .ForMember(d => d.AllowFriendRequests, opt => opt.MapFrom(s => s.AllowFriendRequests))
            .ForMember(d => d.DefaultOutfitPrivacy, opt => opt.MapFrom(s => s.DefaultOutfitPrivacy.ToString()))
            .ForMember(d => d.ShowBodyMetrics, opt => opt.MapFrom(s => s.ShowBodyMetrics))
            .ForMember(d => d.AllowLocationTracking, opt => opt.MapFrom(s => s.AllowLocationTracking));

        // Update DTOs to Entities
        CreateMap<UpdateUserProfileDto, User>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.ProfilePictureUrl, opt => opt.Ignore());

        CreateMap<UpdateStyleProfileDto, UserStyleProfile>()
            .ForMember(d => d.Style, opt => opt.MapFrom(s => s.Style))
            .ForMember(d => d.PreferredColors, opt => opt.MapFrom(s => s.PreferredColors))
            .ForMember(d => d.FitPreferences, opt => opt.MapFrom(s => s.FitPreferences))
            .ForMember(d => d.ComfortPriority, opt => opt.MapFrom(s => s.ComfortPriority))
            .ForMember(d => d.AcceptsTrends, opt => opt.MapFrom(s => s.AcceptsTrends));

        CreateMap<UpdateUserPreferencesDto, UserPreferences>()
            .ForMember(d => d.ShareOutfitsAnonymously, opt => opt.MapFrom(s => s.ShareOutfitsAnonymously))
            .ForMember(d => d.IncludeInTrendAnalysis, opt => opt.MapFrom(s => s.IncludeInTrendAnalysis))
            .ForMember(d => d.AllowFriendRequests, opt => opt.MapFrom(s => s.AllowFriendRequests))
            .ForMember(d => d.DefaultOutfitPrivacy, opt => opt.MapFrom(s => s.DefaultOutfitPrivacy))
            .ForMember(d => d.ShowBodyMetrics, opt => opt.MapFrom(s => s.ShowBodyMetrics))
            .ForMember(d => d.AllowLocationTracking, opt => opt.MapFrom(s => s.AllowLocationTracking));

        // ====================
        // SOCIAL MAPPINGS
        // ====================
        
        // ValidationPoll
        CreateMap<ValidationPoll, ValidationPollDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<ValidationPollDto, ValidationPoll>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => Enum.Parse<PollStatus>(s.Status, true)));

        // PollOption
        CreateMap<PollOption, PollOptionDto>()
            .ForMember(d => d.VoteCount, opt => opt.MapFrom(s => s.Votes.Count));

        CreateMap<PollOptionDto, PollOption>();

        // Vote
        CreateMap<Vote, VoteDto>()
            .ForMember(d => d.Rating, opt => opt.MapFrom(s => s.Rating));

        CreateMap<VoteDto, Vote>();

        // TrendingOutfit
        CreateMap<TrendingOutfit, TrendingOutfitDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.OutfitId))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Outfit.Name))
            .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.Outfit.ImageUrl))
            .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.Outfit.UserId))
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.Outfit.User.Name))
            .ForMember(d => d.UserAvatar, opt => opt.MapFrom(s => s.Outfit.User.ProfilePictureUrl))
            .ForMember(d => d.VoteCount, opt => opt.MapFrom(s => s.VoteCount))
            .ForMember(d => d.CommentsCount, opt => opt.MapFrom(s => s.CommentsCount))
            .ForMember(d => d.TrendingScore, opt => opt.MapFrom(s => s.TrendingScore))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.Date));

        // ====================
        // CALENDAR MAPPINGS
        // ====================
        
        CreateMap<CalendarEvent, CalendarEventItemDto>()
            .ForMember(d => d.StartTime, opt => opt.MapFrom(s => s.StartTime.HasValue ? s.StartTime.Value.ToString(@"hh\:mm") : null))
            .ForMember(d => d.EndTime, opt => opt.MapFrom(s => s.EndTime.HasValue ? s.EndTime.Value.ToString(@"hh\:mm") : null));

        CreateMap<CalendarEventItemDto, CalendarEvent>()
            .ForMember(d => d.StartTime, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.StartTime) ? TimeSpan.Parse(s.StartTime) : (TimeSpan?)null))
            .ForMember(d => d.EndTime, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.EndTime) ? TimeSpan.Parse(s.EndTime) : (TimeSpan?)null));
        // ====================
// FEED MAPPINGS
// ====================

// FeedPost - Main feed entity
CreateMap<FeedPost, FeedPostDto>()
    .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Name))
    .ForMember(d => d.UserAvatarUrl, opt => opt.MapFrom(s => s.User.ProfilePictureUrl))
    .ForMember(d => d.Outfit, opt => opt.MapFrom(s => s.Outfit)) // Nested mapping
    .ForMember(d => d.Poll, opt => opt.MapFrom(s => s.Poll))     // Nested mapping
    .ForMember(d => d.UserReaction, opt => opt.MapFrom(s => 
        s.Reactions.FirstOrDefault(r => r.UserId == s.UserId) != null ? 
        s.Reactions.FirstOrDefault(r => r.UserId == s.UserId).ReactionType.ToString() : null));

// PostComment
CreateMap<PostComment, PostCommentDto>()
    .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.Name))
    .ForMember(d => d.UserAvatarUrl, opt => opt.MapFrom(s => s.User.ProfilePictureUrl))
    .ForMember(d => d.Replies, opt => opt.MapFrom(s => s.Replies));
     } 
}


