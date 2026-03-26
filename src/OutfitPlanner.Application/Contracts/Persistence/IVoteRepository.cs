using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IVoteRepository : IGenericRepository<Vote>
{
    Task<IEnumerable<Vote>> GetByPollIdAsync(Guid pollId);
    Task<IEnumerable<Vote>> GetByOptionIdAsync(Guid optionId);
    
    /// <summary>
    /// Checks if a user has already voted on a poll using a server-side EXISTS query.
    /// Much more efficient than loading all votes into memory.
    /// </summary>
    Task<bool> HasUserVotedAsync(Guid pollId, string userId);
}
