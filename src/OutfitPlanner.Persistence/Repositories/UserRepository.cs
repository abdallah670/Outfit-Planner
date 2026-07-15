using Microsoft.EntityFrameworkCore;
using OutfitPlanner.Application.Common.Interfaces.Persistence;
using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Persistence.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<IEnumerable<MentionedUserDto>> GetMentionedUsersAsync(IEnumerable<string> userIds)
    {
        return await _dbSet.Where(u => userIds.Contains(u.Id)).Select(u => new MentionedUserDto { UserId = u.Id, UserName = u.UserName }).ToListAsync();
    }

    public async Task<IEnumerable<TaggedUserDto>> GetTaggedUsersAsync(IEnumerable<string> usernames)
    {
        return await _dbSet.Where(u => usernames.Contains(u.UserName)).
        Select(u => new TaggedUserDto { UserId = u.Id.ToString(), UserName = u.UserName ,ProfilePictureUrl=u.ProfilePictureUrl}).ToListAsync();
    }
}

