using AutoMapper;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Domain.Entities;
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
            CreateMap<ClothingItem, ClothingItemDto>().ReverseMap();
            CreateMap<ClothingItem, ClothingItemListDto>().ReverseMap();
         //   CreateMap<ClothingItem, CreateClothingItemDto>().ReverseMap();
         // CreateMap<ClothingItem, UpdateClothingItemDto>().ReverseMap();
            #endregion LeaveRequest

        
        }
    }
}
