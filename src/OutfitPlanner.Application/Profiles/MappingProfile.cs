using AutoMapper;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Wardrobe mappings
        CreateMap<ClothingItem, ClothingItemDto>()
            .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Condition, opt => opt.MapFrom(s => s.Condition.ToString()))
            .ReverseMap();

        CreateMap<ClothingItem, ClothingItemListDto>()
            .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type.ToString()));

        // Outfit mappings
        CreateMap<Outfit, OutfitDto>()
            .ForMember(d => d.Occasion, opt => opt.MapFrom(s => s.Occasion.ToString()))
            .ForMember(d => d.Season, opt => opt.MapFrom(s => s.Season.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<CreateOutfitDto, Outfit>()
            .ForMember(d => d.Occasion, opt => opt.MapFrom(s => Enum.Parse<OccasionType>(s.Occasion, true)))
            .ForMember(d => d.Season, opt => opt.MapFrom(s => Enum.Parse<Season>(s.Season, true)))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => OutfitStatus.Active))
            .ForMember(d => d.ImageUrl, opt => opt.Ignore()); // ImageUrl will be set after image generation

        CreateMap<OutfitDto, Outfit>()
            .ForMember(d => d.Occasion, opt => opt.MapFrom(s => Enum.Parse<OccasionType>(s.Occasion, true)))
            .ForMember(d => d.Season, opt => opt.MapFrom(s => Enum.Parse<Season>(s.Season, true)))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => Enum.Parse<OutfitStatus>(s.Status, true)))
            .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl));

        CreateMap<UpdateOutfitDto, Outfit>()
            .ForMember(d => d.Occasion, opt => opt.MapFrom(s => s.Occasion != null ? Enum.Parse<OccasionType>(s.Occasion, true) : default(OccasionType)))
            .ForMember(d => d.Season, opt => opt.MapFrom(s => s.Season != null ? Enum.Parse<Season>(s.Season, true) : default(Season)))
            .ForMember(d => d.Status, opt => opt.Ignore()) // Status should not be updated via this DTO
            .ForMember(d => d.UserId, opt => opt.Ignore()) // UserId should not be changed
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.TimesWorn, opt => opt.Ignore())
            .ForMember(d => d.LastWorn, opt => opt.Ignore())
            .ForMember(d => d.ComfortRating, opt => opt.Condition(s => s.ComfortRating.HasValue))
            .ForMember(d => d.StyleRating, opt => opt.Condition(s => s.StyleRating.HasValue))
            .ForMember(d => d.Name, opt => opt.Condition(s => !string.IsNullOrEmpty(s.Name)))
            .ForMember(d => d.WeatherCondition, opt => opt.Condition(s => !string.IsNullOrEmpty(s.WeatherCondition)))
            .ForMember(d => d.ImageUrl, opt => opt.Condition(s => !string.IsNullOrEmpty(s.ImageUrl))); // Allow ImageUrl to be updated

        CreateMap<Outfit, OutfitListDto>()
            .ForMember(d => d.Occasion, opt => opt.MapFrom(s => s.Occasion.ToString()))
            .ForMember(d => d.Season, opt => opt.MapFrom(s => s.Season.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ItemCount, opt => opt.MapFrom(s => s.Items.Count))
            .ForMember(d => d.ThumbnailUrl, opt => opt.MapFrom(s => s.Items.FirstOrDefault() != null ? s.Items.FirstOrDefault().ClothingItem.ThumbnailUrl : string.Empty));

        CreateMap<OutfitItem, OutfitItemDto>()
            .ForMember(d => d.ClothingItemName, opt => opt.MapFrom(s => s.ClothingItem.Name))
            .ForMember(d => d.ClothingItemImageUrl, opt => opt.MapFrom(s => s.ClothingItem.ImageUrl))
            .ForMember(d => d.ClothingItemType, opt => opt.MapFrom(s => s.ClothingItem.Type))
            .ForMember(d => d.ClothingItemCategory, opt => opt.MapFrom(s => s.ClothingItem.Category.ToString()))
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()));

        // Social mappings
        CreateMap<ValidationPoll, ValidationPollDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<ValidationPollDto, ValidationPoll>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => Enum.Parse<PollStatus>(s.Status, true)));

        CreateMap<PollOption, PollOptionDto>()
            .ForMember(d => d.VoteCount, opt => opt.MapFrom(s => s.Votes.Count));

        CreateMap<PollOptionDto, PollOption>();

        CreateMap<Vote, VoteDto>()
            .ForMember(d => d.Rating, opt => opt.MapFrom(s => s.Rating));

        CreateMap<VoteDto, Vote>();

        CreateMap<TrendingOutfit, TrendingOutfitDto>();

        // Calendar mappings
        CreateMap<CalendarEvent, CalendarEventItemDto>()
            .ForMember(d => d.StartTime, opt => opt.MapFrom(s => s.StartTime.HasValue ? s.StartTime.Value.ToString(@"hh\:mm") : null))
            .ForMember(d => d.EndTime, opt => opt.MapFrom(s => s.EndTime.HasValue ? s.EndTime.Value.ToString(@"hh\:mm") : null));

        CreateMap<CalendarEventItemDto, CalendarEvent>()
            .ForMember(d => d.StartTime, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.StartTime) ? TimeSpan.Parse(s.StartTime) : (TimeSpan?)null))
            .ForMember(d => d.EndTime, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.EndTime) ? TimeSpan.Parse(s.EndTime) : (TimeSpan?)null));
    }
}
