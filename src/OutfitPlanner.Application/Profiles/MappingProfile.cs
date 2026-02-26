using AutoMapper;
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
            #endregion LeaveRequest

        
        }
    }
}
