using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Features.Calendar.Requests.Queries;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Queries;

public class GetScheduledOutfitsRequestHandler : IRequestHandler<GetScheduledOutfitsRequest, List<ScheduledOutfitDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetScheduledOutfitsRequestHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ScheduledOutfitDto>> Handle(GetScheduledOutfitsRequest request, CancellationToken cancellationToken)
    {
        var startDate = new DateTimeOffset(new DateTime(request.Year, request.Month, 1));
        var endDate = startDate.AddMonths(1);
        var now = DateTimeOffset.UtcNow;

        var wearEvents = await _unitOfWork.WearEvents
            .FindAsync(we => we.UserId == request.UserId 
                && we.WornAt >= startDate 
                && we.WornAt < endDate
                && we.WornAt > now);

        var dtos = new List<ScheduledOutfitDto>();
        foreach (var we in wearEvents)
        {
            var outfit = we.OutfitId.HasValue ? await _unitOfWork.Outfits.GetByIdAsync(we.OutfitId.Value) : null;
            var firstItem = outfit?.Items.FirstOrDefault();
            var clothingItem = firstItem?.ClothingItem;
            
            dtos.Add(new ScheduledOutfitDto
            {
                Id = we.Id,
                OutfitId = we.OutfitId ?? Guid.Empty,
                OutfitName = outfit?.Name ?? "Unknown",
                OutfitImageUrl = clothingItem?.ImageUrl,
                Occasion = outfit?.Occasion.ToString(),
                ScheduledDate = we.WornAt,
                Notes = we.Notes,
                Worn = false
            });
        }

        return dtos.OrderBy(e => e.ScheduledDate).ToList();
    }
}
