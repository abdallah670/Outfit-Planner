using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Exceptions;
using OutfitPlanner.Application.Features.ClothingItems.Requests.Queries;

namespace OutfitPlanner.Application.Features.ClothingItems.Handlers.Queries;

public class GetClothingItemByIdRequestHandler : IRequestHandler<GetClothingItemByIdRequest, ClothingItemDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClothingItemByIdRequestHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ClothingItemDto> Handle(GetClothingItemByIdRequest request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.ClothingItems.GetByIdAsync(request.Id);

        if (item == null)
            throw new NotFoundException("Clothing item", request.Id);

        if (item.UserId != request.UserId)
            throw new Exceptions.UnauthorizedAccessException("You do not have access to this clothing item");

        // Simple ad-hoc mapping since AutoMapper might not be fully configured for all DTOs yet
        // If AutoMapper is preferred, it would be _mapper.Map<ClothingItemDto>(item);
        return new ClothingItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Type = item.Type.ToString(),
            Category = item.Category,
            Brand = item.Brand,
            PrimaryColor = item.PrimaryColor,
            SecondaryColors = item.SecondaryColors,
            Fabric = item.Fabric.ToString(),
            PurchasePrice = item.PurchasePrice.Amount, // Assuming Money value object
            Currency = item.PurchasePrice.Currency,
            Size = item.Size,
            Condition = item.Condition,
            ImageUrl = item.ImageUrl,
            ThumbnailUrl = item.ThumbnailUrl,
            LastWorn = item.LastWorn,
            WearCount = item.WearCount
        };
    }
}
