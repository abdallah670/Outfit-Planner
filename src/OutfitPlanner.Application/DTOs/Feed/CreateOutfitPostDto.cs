using System;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.Feed;

public class CreateOutfitPostDto
{
    public Guid OutfitId { get; set; }
    public string? Caption { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Public;
}
