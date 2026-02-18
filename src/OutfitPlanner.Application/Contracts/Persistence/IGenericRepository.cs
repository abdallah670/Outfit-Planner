using System.Linq.Expressions;
using OutfitPlanner.Domain.Entities;

namespace OutfitPlanner.Application.Common.Interfaces.Persistence;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task RemoveAsync(T entity);
    Task RemoveRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task<int> CountAsync();
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize);
    IQueryable<T> Get(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null);
}
