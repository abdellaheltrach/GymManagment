using GymManagement.Domain.Bases;
using System.Linq.Expressions;

namespace GymManagement.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    public Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    public Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    public Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    public Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default);

    public Task AddAsync(T entity, CancellationToken ct = default);

    public void Update(T entity);

    public void Remove(T entity);
}
