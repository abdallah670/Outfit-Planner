using System;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.Feed;

public class CreatePollPostDto
{
    public string Question { get; set; } = string.Empty;
    public List<Guid> OutfitIds { get; set; } = new();
    public DateTimeOffset ExpiresAt { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Public;
    public List<PollOptionDto> Options { get; set; }
 public List<string> Tags { get; set; }
    public string Context { get; set; }
}
