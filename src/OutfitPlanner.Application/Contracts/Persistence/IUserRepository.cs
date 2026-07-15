using OutfitPlanner.Application.DTOs.Feed;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<TaggedUserDto>> GetTaggedUsersAsync(IEnumerable<string> usernames);
    Task<IEnumerable<MentionedUserDto>> GetMentionedUsersAsync(IEnumerable<string> userIds);
}
