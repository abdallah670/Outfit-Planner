using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IVoteReactionRepository : IGenericRepository<VoteReaction>
{
    Task<VoteReaction?> GetUserReactionForVoteAsync(Guid voteId, string userId);
    Task<IEnumerable<VoteReaction>> GetReactionsForVoteAsync(Guid voteId);
}
