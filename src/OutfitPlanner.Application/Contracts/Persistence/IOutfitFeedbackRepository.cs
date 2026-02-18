using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IOutfitFeedbackRepository : IGenericRepository<OutfitFeedback>
{
    Task<IEnumerable<OutfitFeedback>> GetByOutfitIdAsync(Guid outfitId);
}
