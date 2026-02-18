namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Queries{

public class GetClothingItemByIdRequest : IRequest<ClothingItemDto>
{
    public Guid Id { get; set; }
}
}