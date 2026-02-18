using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class UserPreferencesRepository : GenericRepository<UserPreferences>, IUserPreferencesRepository
{
    public UserPreferencesRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<UserPreferences?> GetByUserIdAsync(string userId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);
    }
}
