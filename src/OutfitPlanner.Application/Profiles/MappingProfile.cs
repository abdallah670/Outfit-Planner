using AutoMapper;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutfitPlanner.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Wardrobe Mappings
            CreateMap<ClothingItem, ClothingItemDto>()
                .ForMember(d => d.PurchasePrice, opt => opt.MapFrom(s => s.PurchasePrice != null ? s.PurchasePrice.Amount : 0))
                .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.PurchasePrice != null ? s.PurchasePrice.Currency : "USD"))
                .ReverseMap()
                .ForMember(d => d.PurchasePrice, opt => opt.MapFrom(s => Money.From(s.PurchasePrice, s.Currency)))
                .ForMember(d => d.Type, opt => opt.MapFrom(s => Enum.Parse<ClothingType>(s.Type, true)))
                .ForMember(d => d.Fabric, opt => opt.MapFrom(s => Enum.Parse<FabricType>(s.Fabric, true)));

            CreateMap<ClothingItem, ClothingItemListDto>().ReverseMap();
            CreateMap<ClothingTag, ClothingTagDto>().ReverseMap();

            CreateMap<CreateClothingItemDto, ClothingItem>()
                .ForMember(d => d.PurchasePrice, opt => opt.MapFrom(s => Money.From(s.PurchasePrice, s.Currency)))
                .ForMember(d => d.Type, opt => opt.MapFrom(s => Enum.Parse<ClothingType>(s.Type, true)))
                .ForMember(d => d.Fabric, opt => opt.MapFrom(s => Enum.Parse<FabricType>(s.Fabric, true)));

            CreateMap<UpdateClothingItemDto, ClothingItem>()
                .ForMember(d => d.PurchasePrice, opt => opt.MapFrom(s => Money.From(s.PurchasePrice, s.Currency)))
                .ForMember(d => d.Type, opt => opt.MapFrom(s => Enum.Parse<ClothingType>(s.Type, true)))
                .ForMember(d => d.Fabric, opt => opt.MapFrom(s => Enum.Parse<FabricType>(s.Fabric, true)));
            #endregion

            #region Outfit Mappings
            CreateMap<Outfit, OutfitDto>()
                .ForMember(d => d.Occasion, opt => opt.MapFrom(s => s.Occasion.ToString()))
                .ForMember(d => d.Season, opt => opt.MapFrom(s => s.Season.ToString()))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ReverseMap()
                .ForMember(d => d.Occasion, opt => opt.MapFrom(s => Enum.Parse<OccasionType>(s.Occasion, true)))
                .ForMember(d => d.Season, opt => opt.MapFrom(s => Enum.Parse<Season>(s.Season, true)))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => Enum.Parse<OutfitStatus>(s.Status, true)));

            CreateMap<Outfit, OutfitListDto>()
                .ForMember(d => d.Occasion, opt => opt.MapFrom(s => s.Occasion.ToString()))
                .ForMember(d => d.Season, opt => opt.MapFrom(s => s.Season.ToString()))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.ItemCount, opt => opt.MapFrom(s => s.Items.Count))
                .ForMember(d => d.ThumbnailUrl, opt => opt.MapFrom(s => s.Items.FirstOrDefault() != null ? s.Items.FirstOrDefault().ClothingItem.ThumbnailUrl : string.Empty));

            CreateMap<OutfitItem, OutfitItemDto>()
                .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()))
                .ForMember(d => d.ClothingItemName, opt => opt.MapFrom(s => s.ClothingItem.Name))
                .ForMember(d => d.ClothingItemImageUrl, opt => opt.MapFrom(s => s.ClothingItem.ImageUrl))
                .ForMember(d => d.ClothingItemType, opt => opt.MapFrom(s => s.ClothingItem.Type))
                .ForMember(d => d.ClothingItemCategory, opt => opt.MapFrom(s => s.ClothingItem.Category));

            CreateMap<CreateOutfitDto, Outfit>()
                .ForMember(d => d.Occasion, opt => opt.MapFrom(s => Enum.Parse<OccasionType>(s.Occasion, true)))
                .ForMember(d => d.Season, opt => opt.MapFrom(s => Enum.Parse<Season>(s.Season, true)))
                .ForMember(d => d.Items, opt => opt.Ignore());

            CreateMap<CreateOutfitItemDto, OutfitItem>()
                .ForMember(d => d.Role, opt => opt.MapFrom(s => Enum.Parse<ItemRole>(s.Role, true)));

            CreateMap<UpdateOutfitDto, Outfit>()
                .ForMember(d => d.Occasion, opt => opt.MapFrom((src, dest) => 
                    !string.IsNullOrEmpty(src.Occasion) && Enum.TryParse<OccasionType>(src.Occasion, true, out var occasion) ? occasion : dest.Occasion))
                .ForMember(d => d.Season, opt => opt.MapFrom((src, dest) => 
                    !string.IsNullOrEmpty(src.Season) && Enum.TryParse<Season>(src.Season, true, out var season) ? season : dest.Season))
                .ForMember(d => d.WeatherCondition, opt => opt.MapFrom((src, dest) => 
                    src.WeatherCondition ?? dest.WeatherCondition))
                .ForMember(d => d.Name, opt => opt.MapFrom((src, dest) => 
                    !string.IsNullOrEmpty(src.Name) ? src.Name : dest.Name))
                .ForMember(d => d.ComfortRating, opt => opt.MapFrom((src, dest) => 
                    src.ComfortRating.HasValue ? src.ComfortRating : dest.ComfortRating))
                .ForMember(d => d.StyleRating, opt => opt.MapFrom((src, dest) => 
                    src.StyleRating.HasValue ? src.StyleRating : dest.StyleRating))
                .ForMember(d => d.Items, opt => opt.Ignore());
            #endregion

            #region Social Mappings
            CreateMap<ValidationPoll, ValidationPollDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.TotalVotes, opt => opt.MapFrom(s => s.Votes.Count));

            CreateMap<PollOption, PollOptionDto>()
                .ForMember(d => d.VoteCount, opt => opt.MapFrom(s => s.Votes.Count));

            CreateMap<Vote, VoteDto>();

            CreateMap<CreatePollDto, ValidationPoll>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => PollStatus.Active))
                .ForMember(d => d.Options, opt => opt.Ignore())
                .ForMember(d => d.Votes, opt => opt.Ignore())
                .ForMember(d => d.User, opt => opt.Ignore());

            CreateMap<CreatePollOptionDto, PollOption>()
                .ForMember(d => d.Poll, opt => opt.Ignore())
                .ForMember(d => d.Outfit, opt => opt.Ignore())
                .ForMember(d => d.Votes, opt => opt.Ignore());

            CreateMap<CastVoteDto, Vote>()
                .ForMember(d => d.Option, opt => opt.Ignore())
                .ForMember(d => d.Voter, opt => opt.Ignore());
            #endregion
        }
    }
}
