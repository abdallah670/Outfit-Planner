using System;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.Feed;

public class OutfitDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Occasion { get; set; } = string.Empty;
}