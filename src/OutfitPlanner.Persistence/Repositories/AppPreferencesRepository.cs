using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Contracts.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class AppPreferencesRepository : GenericRepository<AppPreferences>, IAppPreferencesRepository
{
    public AppPreferencesRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<AppPreferences?> GetByUserIdAsync(string userId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);
    }
}
