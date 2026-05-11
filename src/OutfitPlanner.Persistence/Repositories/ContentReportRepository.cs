using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class ContentReportRepository : GenericRepository<ContentReport>, IContentReportRepository
{
    public ContentReportRepository(AppDbContext context) : base(context)
    {
    }
}
