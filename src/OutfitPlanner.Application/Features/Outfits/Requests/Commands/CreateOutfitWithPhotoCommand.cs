using MediatR;
using Microsoft.AspNetCore.Http;
using OutfitPlanner.Application.DTOs.Outfit;

namespace OutfitPlanner.Application.Features.Outfits.Requests.Commands;

/// <summary>
/// Command to create a new outfit with a photo upload (no clothing items required)
/// </summary>
public class CreateOutfitWithPhotoCommand : IRequest<CreateOutfitWithPhotoResponseDto>
{
    public string UserId { get; set; } = string.Empty;
    public IFormFile Photo { get; set; } = null!;
    public string? Name { get;  set; }
}
