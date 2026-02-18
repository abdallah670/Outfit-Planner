using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IValidationPollRepository : IGenericRepository<ValidationPoll>
{
    Task<IEnumerable<ValidationPoll>> GetActivePollsAsync();
    Task<IEnumerable<ValidationPoll>> GetByUserIdAsync(string userId);
}
