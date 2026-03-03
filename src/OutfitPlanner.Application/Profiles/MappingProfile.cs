using AutoMapper;
using OutfitPlanner.Application.DTOs.Outfit;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Domain.ValueObjects;
using System;
using System.Collections.Generic;
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
                .ForMember(d => d.Type, opt => opt.MapFrom(s => Enum.Parse<ClothingType>(s.Type)))
                .ForMember(d => d.Fabric, opt => opt.MapFrom(s => Enum.Parse<FabricType>(s.Fabric)));

            CreateMap<ClothingItem, ClothingItemListDto>().ReverseMap();
            CreateMap<ClothingTag, ClothingTagDto>().ReverseMap();

            CreateMap<CreateClothingItemDto, ClothingItem>()
                .ForMember(d => d.PurchasePrice, opt => opt.MapFrom(s => Money.From(s.PurchasePrice, s.Currency)))
                .ForMember(d => d.Type, opt => opt.MapFrom(s => Enum.Parse<ClothingType>(s.Type)))
                .ForMember(d => d.Fabric, opt => opt.MapFrom(s => Enum.Parse<FabricType>(s.Fabric)));

            CreateMap<UpdateClothingItemDto, ClothingItem>()
                .ForMember(d => d.PurchasePrice, opt => opt.MapFrom(s => Money.From(s.PurchasePrice, s.Currency)))
                .ForMember(d => d.Type, opt => opt.MapFrom(s => Enum.Parse<ClothingType>(s.Type)))
                .ForMember(d => d.Fabric, opt => opt.MapFrom(s => Enum.Parse<FabricType>(s.Fabric)));
            #endregion

            #region Outfit Mappings
            CreateMap<Outfit, OutfitDto>()
                .ForMember(d => d.Occasion, opt => opt.MapFrom(s => s.Occasion.ToString()))
                .ForMember(d => d.Season, opt => opt.MapFrom(s => s.Season.ToString()))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ReverseMap()
                .ForMember(d => d.Occasion, opt => opt.MapFrom(s => Enum.Parse<OccasionType>(s.Occasion)))
                .ForMember(d => d.Season, opt => opt.MapFrom(s => Enum.Parse<Season>(s.Season)))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => Enum.Parse<OutfitStatus>(s.Status)));

            CreateMap<Outfit, OutfitListDto>()
                .ForMember(d => d.Occasion, opt => opt.MapFrom(s => s.Occasion.ToString()))
                .ForMember(d => d.Season, opt => opt.MapFrom(s => s.Season.ToString()))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.ItemCount, opt => opt.MapFrom(s => s.Items.Count))
                .ForMember(d => d.ThumbnailUrl, opt => opt.MapFrom(s => s.Items.FirstOrDefault() != null ? s.Items.FirstOrDefault().ClothingItem.ThumbnailUrl : string.Empty));

            CreateMap<OutfitItem, OutfitItemDto>()
                .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()))
                .ForMember(d => d.ClothingItemName, opt => opt.MapFrom(s => s.ClothingItem.Name))
                .ForMember(d => d.ClothingItemImageUrl, opt => opt.MapFrom(s => s.ClothingItem.ImageUrl));

            CreateMap<CreateOutfitDto, Outfit>()
                .ForMember(d => d.Occasion, opt => opt.MapFrom(s => Enum.Parse<OccasionType>(s.Occasion, true)))
                .ForMember(d => d.Season, opt => opt.MapFrom(s => Enum.Parse<Season>(s.Season, true)));

            CreateMap<CreateOutfitItemDto, OutfitItem>()
                .ForMember(d => d.Role, opt => opt.MapFrom(s => Enum.Parse<ItemRole>(s.Role, true)));

            CreateMap<UpdateOutfitDto, Outfit>()
                .ForMember(d => d.Occasion, opt => opt.Condition((src, dest, srcMember) => srcMember != null))
                .ForMember(d => d.Occasion, opt => opt.MapFrom((src, dest) => src.Occasion != null ? Enum.Parse<OccasionType>(src.Occasion) : dest.Occasion))
                .ForMember(d => d.Season, opt => opt.Condition((src, dest, srcMember) => srcMember != null))
                .ForMember(d => d.Season, opt => opt.MapFrom((src, dest) => src.Season != null ? Enum.Parse<Season>(src.Season) : dest.Season));
            #endregion
        }
    }
}
