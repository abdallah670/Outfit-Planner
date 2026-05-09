using System;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Application.DTOs.Outfit;

namespace OutfitPlanner.Application.DTOs.Feed;

public class FeedPostDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserAvatarUrl { get; set; } = string.Empty;
    public PostType PostType { get; set; }
    public Guid? OutfitId { get; set; }
    public OutfitDto? Outfit { get; set; }
    public Guid? PollId { get; set; }
    public ValidationPollDto? Poll { get; set; }
    public string? Caption { get; set; }
    public Visibility Visibility { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public string? UserReaction { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
