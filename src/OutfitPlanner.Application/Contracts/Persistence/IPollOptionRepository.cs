using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IPollOptionRepository : IGenericRepository<PollOption>
{
    Task<IEnumerable<PollOption>> GetByPollIdAsync(Guid pollId);
}
