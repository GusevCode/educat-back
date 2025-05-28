using System.Linq.Expressions;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Interfaces;

public interface IRepository<T, TKey> where T : BaseEntity
{
    Task<T?> GetByIdAsync(TKey id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(TKey id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}

public interface IRepository<T> : IRepository<T, int> where T : BaseEntity
{
} 