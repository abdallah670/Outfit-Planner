using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class SystemSettingRepository : GenericRepository<SystemSetting>, ISystemSettingRepository
{
    public SystemSettingRepository(AppDbContext context) : base(context)
    {
    }
}
