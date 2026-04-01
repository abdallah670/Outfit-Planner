using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Contracts.Persistence;

public interface IValidationPollRepository : IGenericRepository<ValidationPoll>
{
    Task<IEnumerable<ValidationPoll>> GetActivePollsAsync();
    Task<IEnumerable<ValidationPoll>> GetByUserIdAsync(string userId);
    Task<IEnumerable<ValidationPoll>> GetPollsForTrendingAsync();
}
