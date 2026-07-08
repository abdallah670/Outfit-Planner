using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OutfitPlanner.Application.Contracts.Infrastructure;

namespace OutfitPlanner.Infrastructure.Services;

/// <summary>
/// Implementation of ISocialHubService for pushing real-time social updates.
/// </summary>
public class SocialHubService : ISocialHubService
{
    private readonly IHubContext<SocialHub> _hubContext;
    private readonly ILogger<SocialHubService> _logger;

    public SocialHubService(IHubContext<SocialHub> hubContext, ILogger<SocialHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyNewPostAsync(string postOwnerId, object postDto)
    {
        await _hubContext.Clients.Group($"feed_{postOwnerId}").SendAsync("NewPost", postDto);
        _logger.LogInformation("SocialHubService: Notified followers of new post by {UserId}", postOwnerId);
    }

    public async Task NotifyAllNewPostAsync(object postDto)
    {
        await _hubContext.Clients.All.SendAsync("NewPost", postDto);
        _logger.LogInformation("SocialHubService: Notified all users of new post");
    }

    public async Task NotifyCommentUpdateAsync(string postId, int commentCount)
    {
        await _hubContext.Clients.All.SendAsync("CommentUpdate", postId, commentCount);
        _logger.LogInformation("SocialHubService: Sent comment update for post {PostId}: {Count} comments", 
            postId, commentCount);
    }

    public async Task NotifyReactionUpdateAsync(string postId, int reactionCount)
    {
        await _hubContext.Clients.All.SendAsync("ReactionUpdate", postId, reactionCount);
        _logger.LogInformation("SocialHubService: Sent reaction update for post {PostId}: {Count} reactions", 
            postId, reactionCount);
    }
}