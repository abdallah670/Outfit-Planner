using MediatR;
using OutfitPlanner.Application.DTOs.Wardrobe;
using OutfitPlanner.Application.Responses;

namespace OutfitPlanner.Application.Features.ClothingItems.Requests.Commands;

/// <summary>
/// Command to record a wear event for a clothing item
/// </summary>
public class RecordWearCommand : IRequest<BaseCommandResponse>
{
    public string UserId { get; set; } = string.Empty;
    public required RecordWearDto Request { get; set; }
}