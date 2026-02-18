using AutoMapper;

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
            CreateMap<ClothingItem, ClothingItemListDto>()
                .ForMember(dest => dest.DateRequested, opt => opt.MapFrom(src => src.DateCreated))
                .ReverseMap();
         //   CreateMap<ClothingItem, CreateClothingItemDto>().ReverseMap();
         // CreateMap<ClothingItem, UpdateClothingItemDto>().ReverseMap();
            #endregion LeaveRequest

        
        }
    }
}
