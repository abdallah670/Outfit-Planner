using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.DTOs.Social;
using OutfitPlanner.Domain.Entities;
using OutfitPlanner.Domain.Enums;
using OutfitPlanner.Persistence;

namespace OutfitPlanner.Api.Controllers;

/// <summary>
/// Controller for outfit voting and engagement (like, comment, react)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OutfitPollController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<OutfitPollController> _logger;

    public OutfitPollController(AppDbContext context, ILogger<OutfitPollController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Like an outfit (creates a vote with rating 5)
    /// </summary>
    [HttpPost("outfits/{outfitId:guid}/like")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OutfitVoteResultDto>> LikeOutfit(Guid outfitId)
    {
        var userId = GetUserId();
        
        // Find or create outfit poll
        var poll = await GetOrCreateOutfitPollAsync(outfitId);
        if (poll == null)
            return NotFound("Outfit not found");

        var option = poll.Options.FirstOrDefault();
        if (option == null)
            return BadRequest("Invalid outfit poll configuration");

        // Check if user already voted
        var existingVote = await _context.Votes
            .FirstOrDefaultAsync(v => v.OptionId == option.Id && v.VoterId == userId);

        if (existingVote != null)
        {
            // Already liked, return current state
            return Ok(new OutfitVoteResultDto
            {
                OutfitId = outfitId,
                VoteCount = await GetVoteCountAsync(option.Id),
                UserHasVoted = true
            });
        }

        // Create vote (like)
        var vote = new Vote
        {
            PollId = poll.Id,
            OptionId = option.Id,
            VoterId = userId,
            Rating = 5, // Like = 5 stars
            IsAnonymous = false
        };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} liked outfit {OutfitId}", userId, outfitId);

        return Ok(new OutfitVoteResultDto
        {
            OutfitId = outfitId,
            VoteCount = await GetVoteCountAsync(option.Id),
            UserHasVoted = true
        });
    }

    /// <summary>
    /// Unlike an outfit (removes vote)
    /// </summary>
    [HttpDelete("outfits/{outfitId:guid}/like")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OutfitVoteResultDto>> UnlikeOutfit(Guid outfitId)
    {
        var userId = GetUserId();

        var poll = await _context.ValidationPolls
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Context.Contains($"\"outfitId\":\"{outfitId}\"") || 
                                       p.Options.Any(o => o.OutfitId == outfitId));

        if (poll == null)
            return NotFound();

        var option = poll.Options.FirstOrDefault();
        if (option == null)
            return NotFound();

        var vote = await _context.Votes
            .FirstOrDefaultAsync(v => v.OptionId == option.Id && v.VoterId == userId);

        if (vote != null)
        {
            _context.Votes.Remove(vote);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} unliked outfit {OutfitId}", userId, outfitId);
        }

        return Ok(new OutfitVoteResultDto
        {
            OutfitId = outfitId,
            VoteCount = await GetVoteCountAsync(option.Id),
            UserHasVoted = false
        });
    }

    /// <summary>
    /// Comment on an outfit (creates a vote with comment)
    /// </summary>
    [HttpPost("outfits/{outfitId:guid}/comment")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VoteCommentDto>> CommentOnOutfit(Guid outfitId, [FromBody] CreateCommentRequest request)
    {
        var userId = GetUserId();

        var poll = await GetOrCreateOutfitPollAsync(outfitId);
        if (poll == null)
            return NotFound("Outfit not found");

        var option = poll.Options.FirstOrDefault();
        if (option == null)
            return BadRequest("Invalid outfit poll configuration");

        // Get voter info
        var user = await _context.Users.FindAsync(userId);

        // Create vote with comment
        var vote = new Vote
        {
            PollId = poll.Id,
            OptionId = option.Id,
            VoterId = userId,
            Rating = 5,
            Comment = request.Content,
            IsAnonymous = false
        };

        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} commented on outfit {OutfitId}", userId, outfitId);

        return CreatedAtAction(nameof(GetOutfitVotes), new { outfitId }, new VoteCommentDto
        {
            Id = vote.Id,
            UserName = user?.UserName ?? "Anonymous",
            Content = vote.Comment!,
            Rating = vote.Rating,
            CreatedAt = vote.CreatedAt,
            Reactions = new List<VoteReactionDto>()
        });
    }

    /// <summary>
    /// Get all votes/comments for an outfit
    /// </summary>
    [HttpGet("outfits/{outfitId:guid}/votes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<VoteCommentDto>>> GetOutfitVotes(Guid outfitId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var poll = await _context.ValidationPolls
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Options.Any(o => o.OutfitId == outfitId));

        if (poll == null)
            return Ok(new PagedResult<VoteCommentDto> { Items = new List<VoteCommentDto>(), TotalCount = 0, Page = page, PageSize = pageSize });

        var option = poll.Options.FirstOrDefault(o => o.OutfitId == outfitId);
        if (option == null)
            return Ok(new PagedResult<VoteCommentDto> { Items = new List<VoteCommentDto>(), TotalCount = 0, Page = page, PageSize = pageSize });

        var query = _context.Votes
            .Include(v => v.Voter)
            .Include(v => v.Reactions)
            .Where(v => v.OptionId == option.Id && !string.IsNullOrEmpty(v.Comment))
            .OrderByDescending(v => v.CreatedAt);

        var totalCount = await query.CountAsync();
        var votes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = votes.Select(v => new VoteCommentDto
        {
            Id = v.Id,
            UserName = v.Voter?.UserName ?? "Anonymous",
            Content = v.Comment!,
            Rating = v.Rating,
            CreatedAt = v.CreatedAt,
            Reactions = v.Reactions.Select(r => new VoteReactionDto
            {
                UserId = r.UserId,
                ReactionType = r.ReactionType.ToString()
            }).ToList()
        }).ToList();

        return Ok(new PagedResult<VoteCommentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Add reaction to a vote/comment
    /// </summary>
    [HttpPost("votes/{voteId:guid}/react")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ReactToVote(Guid voteId, [FromBody] ReactionRequest request)
    {
        var userId = GetUserId();

        // Validate reaction type
        if (!Enum.TryParse<ReactionType>(request.ReactionType, true, out var reactionType))
            return BadRequest("Invalid reaction type");

        // Remove existing reaction if any
        var existingReaction = await _context.VoteReactions
            .FirstOrDefaultAsync(r => r.VoteId == voteId && r.UserId == userId);

        if (existingReaction != null)
        {
            if (existingReaction.ReactionType == reactionType)
            {
                // Toggle off - remove reaction
                _context.VoteReactions.Remove(existingReaction);
            }
            else
            {
                // Change reaction type
                existingReaction.ReactionType = reactionType;
            }
        }
        else
        {
            // Add new reaction
            _context.VoteReactions.Add(new VoteReaction
            {
                VoteId = voteId,
                UserId = userId,
                ReactionType = reactionType
            });
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Get engagement stats for an outfit
    /// </summary>
    [HttpGet("outfits/{outfitId:guid}/engagement")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<OutfitEngagementDto>> GetEngagement(Guid outfitId)
    {
        var userId = GetUserId();

        var poll = await _context.ValidationPolls
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Options.Any(o => o.OutfitId == outfitId));

        if (poll == null)
            return Ok(new OutfitEngagementDto { VoteCount = 0, CommentCount = 0 });

        var option = poll.Options.FirstOrDefault(o => o.OutfitId == outfitId);
        if (option == null)
            return Ok(new OutfitEngagementDto { VoteCount = 0, CommentCount = 0 });

        var votes = await _context.Votes
            .Where(v => v.OptionId == option.Id)
            .Include(v => v.Reactions)
            .ToListAsync();

        var userVote = votes.FirstOrDefault(v => v.VoterId == userId);

        return Ok(new OutfitEngagementDto
        {
            VoteCount = votes.Count,
            CommentCount = votes.Count(v => !string.IsNullOrEmpty(v.Comment)),
            ReactionCount = await _context.VoteReactions
                .CountAsync(r => votes.Select(v => v.Id).Contains(r.VoteId)),
            UserHasVoted = userVote != null,
            UserReaction = userVote?.Reactions?.FirstOrDefault(r => r.UserId == userId)?.ReactionType.ToString()
        });
    }

    #region Helper Methods

    private async Task<ValidationPoll?> GetOrCreateOutfitPollAsync(Guid outfitId)
    {
        // Find existing poll for this outfit
        var poll = await _context.ValidationPolls
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Context.Contains($"\"outfitId\":\"{outfitId}\"") || 
                                       p.Options.Any(o => o.OutfitId == outfitId));

        if (poll != null)
            return poll;

        // Create new poll for the outfit
        poll = new ValidationPoll
        {
            Id = Guid.NewGuid(),
            Question = "Outfit Rating",
            Context = $"{{\"outfitId\":\"{outfitId}\"}}",
            Status = PollStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            Options = new List<PollOption>
            {
                new PollOption
                {
                    Id = Guid.NewGuid(),
                    OutfitId = outfitId,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        _context.ValidationPolls.Add(poll);
        await _context.SaveChangesAsync();

        return poll;
    }

    private async Task<int> GetVoteCountAsync(Guid optionId)
    {
        return await _context.Votes.CountAsync(v => v.OptionId == optionId);
    }

    #endregion
}
