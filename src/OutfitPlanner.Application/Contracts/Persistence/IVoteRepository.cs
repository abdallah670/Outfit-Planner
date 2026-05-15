using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IVoteRepository : IGenericRepository<Vote>
{
    Task<IEnumerable<Vote>> GetByPollIdAsync(Guid pollId);
    Task<IEnumerable<Vote>> GetByOptionIdAsync(Guid optionId);
    Task<Vote?> GetUserVote(string userId, Guid pollId);
    Task<Vote?> GetUserVoteByOptionId(string userId, Guid optionId);
    Task DeleteVoteAsync(string voterId, Guid optionId);
    /// <summary>
    /// Checks if a user has already voted on a poll using a server-side EXISTS query.
    /// Much more efficient than loading all votes into memory.
    /// </summary>
    Task<bool> HasUserVotedAsync(Guid pollId, string userId);
    /// <summary>
    /// Gets voters with user details (name, avatar) for a poll or specific option
    /// </summary>
    Task<IEnumerable<(Vote Vote, string VoterName, string? VoterAvatarUrl)>> GetVotersWithDetailsAsync(Guid pollId, Guid? optionId = null);
}
