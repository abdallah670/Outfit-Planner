using OutfitPlanner.Application.DTOs.Feed;

namespace OutfitPlanner.Application.Contracts.Infrastructure;

/// <summary>
/// Abstraction for pushing real-time social updates via SignalR.
/// </summary>
public interface ISocialHubService
{
    /// <summary>
    /// Notify all followers of a user about a new post
    /// </summary>
    Task NotifyNewPostAsync(string postOwnerId, object postDto);
    
    /// <summary>
    /// Notify ALL connected users about a new post (for the "All Posts" feed)
    /// </summary>
    Task NotifyAllNewPostAsync(object postDto);
    
    /// <summary>
    /// Notify followers about comment count update on a post
    /// </summary>
    Task NotifyCommentUpdateAsync(string postId, int commentCount);
    
    /// <summary>
    /// Notify followers about reaction count update on a post
    /// </summary>
    Task NotifyReactionUpdateAsync(string postId, int reactionCount);
}
