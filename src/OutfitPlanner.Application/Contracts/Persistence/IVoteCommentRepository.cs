using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IVoteCommentRepository : IGenericRepository<VoteComment>
{
    Task<IEnumerable<VoteComment>> GetCommentsForVoteAsync(Guid voteId, int maxDepth);
}
