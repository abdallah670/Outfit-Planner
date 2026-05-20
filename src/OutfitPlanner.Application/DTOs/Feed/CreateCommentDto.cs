using System;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.Feed;

public class CreateCommentDto
{
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
}
