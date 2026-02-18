namespace OutfitPlanner.Application.Features.ClothingItems.Handlers.Queries{

public class GetClothingItemByIdRequestHanlder : IRequestHandler<GetClothingItemByIdRequest, ClothingItemDto>
{
    private readonly IClothingItemRepository _clothingItemRepository;
    private readonly IMapper _mapper;
    public GetClothingItemByIdRequestHanlder(IClothingItemRepository clothingItemRepository, IMapper mapper)
    {
        _clothingItemRepository = clothingItemRepository;
        _mapper = mapper;
    }

    public async Task<ClothingItemDto> Handle(GetClothingItemByIdRequest request, CancellationToken cancellationToken)
    {
        var clothingItem=await _clothingItemRepository.GetByIdAsync(request.Id);
        return _mapper.Map<ClothingItemDto>(clothingItem);
       
    }
}
}