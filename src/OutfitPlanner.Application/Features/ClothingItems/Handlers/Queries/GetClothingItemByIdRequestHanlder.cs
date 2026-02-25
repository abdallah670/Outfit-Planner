

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Queries;

namespace OutfitPlanner.Application.Features.ClothingItems.Handlers.Queries{

public class GetClothingItemByIdRequestHanlder : IRequestHandler<GetClothingItemByIdRequest, ClothingItemDto>
{
    private readonly IClothingItemRepository _clothingItemRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetClothingItemByIdRequestHanlder> _logger;
    public GetClothingItemByIdRequestHanlder(IClothingItemRepository clothingItemRepository,
    ILogger<GetClothingItemByIdRequestHanlder> logger, IMapper mapper)
    {
        _clothingItemRepository = clothingItemRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ClothingItemDto> Handle(GetClothingItemByIdRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Ensure the clothing item belongs to the requesting user  
            var clothingItem = await _clothingItemRepository.GetByIdAsync(request.Id);
            if (clothingItem == null)
            {
                _logger.LogWarning("Clothing item with ID {ClothingItemId} not found", request.Id);
                throw new NotFoundException("Clothing item", request.Id);
            }
            if (clothingItem.UserId != request.UserId)
            {
                _logger.LogWarning("User {UserId} attempted to access clothing item {ClothingItemId} belonging to another user", request.UserId, request.Id);
                throw new Exceptions.UnauthorizedAccessException("You are not authorized to access this clothing item");
            }   
            
           return _mapper.Map<ClothingItemDto>(clothingItem);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exceptions.UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the clothing item with ID {ClothingItemId}", request.Id);
            throw new BadRequestException("An error occurred while retrieving the clothing item. Please try again later.");
        }
    }
       
    }
}
