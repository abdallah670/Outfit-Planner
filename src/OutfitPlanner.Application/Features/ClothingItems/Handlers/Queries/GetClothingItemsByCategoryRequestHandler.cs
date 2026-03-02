using MediatR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Queries;

namespace OutfitPlanner.Application.Features.ClothingItems.Handlers.Queries;

public class GetClothingItemsByCategoryRequestHandler : IRequestHandler<GetClothingItemsByCategoryRequest, List<ClothingItemListDto>>
{
    private readonly ILogger<GetClothingItemsByCategoryRequestHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    public GetClothingItemsByCategoryRequestHandler(ILogger<GetClothingItemsByCategoryRequestHandler> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
        

    public Task<List<ClothingItemListDto>> Handle(GetClothingItemsByCategoryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var clothingItems = _unitOfWork.ClothingItems.GetByCategoryAsync(request.UserId, request.Category);
            if (clothingItems == null || clothingItems.Result.Count() == 0)
            {
                _logger.LogInformation("No clothing items found for user with ID {UserId} and category {Category}", request.UserId, request.Category);
                return Task.FromResult(new List<ClothingItemListDto>());
            }
            return Task.FromResult(clothingItems.Result.Select(ci => new ClothingItemListDto
            {
                Id = ci.Id,
                Name = ci.Name,
                Category = ci.Category,
                PrimaryColor = ci.PrimaryColor,
                ImageUrl = ci.ImageUrl,
                Type = ci.Type.ToString(),
                ThumbnailUrl = ci.ThumbnailUrl,
                CreatedAt = ci.CreatedAt,
                WearCount = ci.WearCount,
                LastWorn = ci.LastWorn,
                Brand = ci.Brand,
                Condition = ci.Condition.ToString(),
                PurchasePrice = ci.PurchasePrice.Amount
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving clothing items for user with ID {UserId} and category {Category}", request.UserId, request.Category);
            throw new BadRequestException("An error occurred while retrieving clothing items. Please try again later.");
        }   
       
    }
}
