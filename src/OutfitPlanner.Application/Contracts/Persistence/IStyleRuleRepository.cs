using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IStyleRuleRepository : IGenericRepository<StyleRule>
{
    Task<IEnumerable<StyleRule>> GetByProfileIdAsync(Guid profileId);
}
