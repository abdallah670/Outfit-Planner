using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class UserStyleProfileRepository : GenericRepository<UserStyleProfile>, IUserStyleProfileRepository
{
    public UserStyleProfileRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<UserStyleProfile?> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(p => p.CustomRules)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }
}
