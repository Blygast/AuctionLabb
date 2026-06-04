using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Tests.Fakes;

public class InMemoryAuctionRepository : InMemoryRepositoryBase<Auction>, IAuctionRepository
{
    public Task<IEnumerable<Auction>> GetFilteredAsync(string? search, string? status, CancellationToken ct = default)
    {
        IEnumerable<Auction> q = _items;

        q = status switch
        {
            "open"   => q.Where(a => a.IsActive && a.EndDate > DateTime.UtcNow),
            "closed" => q.Where(a => !a.IsActive || a.EndDate <= DateTime.UtcNow),
            "all"    => q,
            _        => q.Where(a => a.IsActive),
        };

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(a => a.Title.Contains(search));

        return Task.FromResult(q.OrderByDescending(a => a.StartDate).AsEnumerable());
    }

    public Task<Auction?> GetWithDetailsAsync(int id, CancellationToken ct = default) =>
        FirstOrNull(a => a.Id == id);

    public Task<IReadOnlyList<Auction>> ListAllAsync(CancellationToken ct = default) =>
        Task.FromResult((IReadOnlyList<Auction>)_items);
}
