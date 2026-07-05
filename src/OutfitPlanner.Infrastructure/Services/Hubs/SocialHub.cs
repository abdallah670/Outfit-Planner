using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Contracts.Persistence;

namespace OutfitPlanner.Infrastructure.Services;

/// <summary>
/// SignalR hub for real-time social feed updates.
/// Users join groups based on who they follow to receive live updates.
/// </summary>
[Authorize]
public class SocialHub : Hub
{
    private readonly ILogger<SocialHub> _logger;
    private readonly IFollowRepository _followRepository;

    public SocialHub(ILogger<SocialHub> logger, IFollowRepository followRepository)
    {
        _logger = logger;
        _followRepository = followRepository;
    }

    /// <summary>
    /// Called when a client connects. Adds user to their personal feed group
    /// and groups for each user they follow.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal group (for direct notifications)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("SocialHub: User {UserId} connected", userId);

            // Add user to groups of all users they follow
            var following = await _followRepository.GetFollowingAsync(userId, 1, 1000);
            foreach (var follow in following)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"feed_{follow.FollowedId}");
            }
            _logger.LogInformation("SocialHub: User {UserId} added to {GroupCount} feed groups", 
                userId, following.Count);
        }
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects. Removes user from all groups.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("SocialHub: User {UserId} disconnected", userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client calls this to explicitly join their feed group.
    /// Useful for reconnection scenarios.
    /// </summary>
    public async Task JoinFeed()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"feed_{userId}");
        }
    }

    /// <summary>
    /// Server-side method to push new post to followers.
    /// </summary>
    public async Task NewPost(string followedUserId, object postDto)
    {
        await Clients.Group($"feed_{followedUserId}").SendAsync("NewPost", postDto);
    }

    /// <summary>
    /// Server-side method to push comment count update.
    /// </summary>
    public async Task CommentUpdate(string postId, int newCount)
    {
        await Clients.All.SendAsync("CommentUpdate", postId, newCount);
    }

    /// <summary>
    /// Server-side method to push reaction count update.
    /// </summary>
    public async Task ReactionUpdate(string postId, int newCount)
    {
        await Clients.All.SendAsync("ReactionUpdate", postId, newCount);
    }
}