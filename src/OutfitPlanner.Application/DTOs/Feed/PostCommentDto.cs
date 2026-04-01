using System;
using System.Collections.Generic;
using OutfitPlanner.Domain.Enums;

namespace OutfitPlanner.Application.DTOs.Feed;

public class PostCommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserAvatarUrl { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public int LikeCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<PostCommentDto> Replies { get; set; } = new();
}

public class PostCommentsResponse
{
    public List<PostCommentDto> Comments { get; set; } = new();
    public int TotalCount { get; set; }
}

public class FollowDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public DateTimeOffset FollowedAt { get; set; }
}

public class FollowersResponse
{
    public List<FollowDto> Followers { get; set; } = new();
    public int TotalCount { get; set; }
}