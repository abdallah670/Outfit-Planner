using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IVoteRepository : IGenericRepository<Vote>
{
    Task<IEnumerable<Vote>> GetByPollIdAsync(Guid pollId);
    Task<IEnumerable<Vote>> GetByOptionIdAsync(Guid optionId);
}
