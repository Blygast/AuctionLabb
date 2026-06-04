using System.Linq.Expressions;
using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Tests.Fakes;

/// <summary>
/// Generic in-memory store for any aggregate root with an int Id.
/// Concrete repository fakes inherit and add their query-specific methods.
/// </summary>
public abstract class InMemoryRepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly List<T> _items = new();
    private int _nextId = 1;

    /// <summary>Test-only view of all stored items.</summary>
    public IReadOnlyList<T> All => _items;

    public virtual Task<T?> GetByIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(_items.FirstOrDefault(x => GetId(x) == id));

    public virtual Task AddAsync(T entity, CancellationToken ct = default)
    {
        if (GetId(entity) == 0) SetId(entity, _nextId++);
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public virtual void Update(T entity) { /* no-op: in-memory reference */ }

    public virtual void Remove(T entity) => _items.Remove(entity);

    // Convenience for tests: linear scan by predicate.
    protected Task<T?> FirstOrNull(Expression<Func<T, bool>> predicate) =>
        Task.FromResult(_items.AsQueryable().FirstOrDefault(predicate));

    private static int GetId(T entity) =>
        (int)typeof(T).GetProperty("Id")!.GetValue(entity)!;

    private static void SetId(T entity, int id) =>
        typeof(T).GetProperty("Id")!.SetValue(entity, id);
}
