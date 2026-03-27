using AutoMapper;
using MediatR;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Calendar;
using OutfitPlanner.Application.Features.Calendar.Requests.Queries;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Features.Calendar.Handlers.Queries;

public class GetCalendarEventsRequestHandler : IRequestHandler<GetCalendarEventsRequest, List<CalendarEventDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCalendarEventsRequestHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CalendarEventDto>> Handle(GetCalendarEventsRequest request, CancellationToken cancellationToken)
    {
        var startDate = new DateTimeOffset(new DateTime(request.Year, request.Month, 1));
        var endDate = startDate.AddMonths(1);

        var wearEvents = await _unitOfWork.WearEvents
            .FindAsync(we => we.UserId == request.UserId && we.WornAt >= startDate && we.WornAt < endDate);

        var events = wearEvents.ToList();
        
        var dtos = new List<CalendarEventDto>();
        foreach (var we in events)
        {
            Outfit? outfit = null;
            if (we.OutfitId.HasValue)
            {
                outfit = await _unitOfWork.Outfits.GetByIdAsync(we.OutfitId.Value);
            }
            
            dtos.Add(new CalendarEventDto
            {
                Id = we.Id,
                UserId = we.UserId,
                OutfitId = we.OutfitId,
                OutfitName = outfit?.Name,
                OutfitImageUrl = outfit?.ImageUrl,
                ClothingItemId = we.ClothingItemId,
                WornAt = we.WornAt,
                DurationMinutes = we.DurationMinutes,
                WeatherCondition = we.WeatherCondition,
                Rating = we.Rating,
                Notes = we.Notes,
                IsScheduled = we.WornAt > DateTimeOffset.UtcNow
            });
        }

        return dtos.OrderBy(e => e.WornAt).ToList();
    }
}
