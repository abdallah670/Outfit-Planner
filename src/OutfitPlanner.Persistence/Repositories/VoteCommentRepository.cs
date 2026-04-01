using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class VoteCommentRepository : GenericRepository<VoteComment>, IVoteCommentRepository
{
    private readonly AppDbContext _context;

    public VoteCommentRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VoteComment>> GetCommentsForVoteAsync(Guid voteId, int maxDepth)
    {
        return await _context.VoteComments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .Where(c => c.VoteId == voteId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
}
