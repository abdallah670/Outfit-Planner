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
    public List<string> Tags { get; set; } = new();
    public string? UserReaction { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsFollowing { get; set; } = false;
    public bool IsOwner { get; set; } = false;
    public bool HasVoted { get; set; } = false;
    public bool IsLiked { get; set; } = false;
    public List<TaggedUserDto> TaggedUsers { get; set; } = new();

}
public class GetFeedPostByIdDto : FeedPostDto
{
    public List<PostCommentDto> Comments { get; set; } = new();

}

public class TaggedUserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}