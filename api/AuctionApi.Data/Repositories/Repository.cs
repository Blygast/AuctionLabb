using System.Linq.Expressions;
using AuctionApi.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuctionApi.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public Task<T?> GetByIdAsync(int id, CancellationToken ct = default) => _set.FindAsync(new object?[] { id }, ct).AsTask();
    public async Task AddAsync(T entity, CancellationToken ct = default) => await _set.AddAsync(entity, ct);
    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);

    // Shared helpers used by the specific repository implementations.
    protected async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        await _set.FirstOrDefaultAsync(predicate);

    protected async Task<List<T>> ToListAsync(IQueryable<T> query) => await query.ToListAsync();
}
