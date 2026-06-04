using AuctionApi.Core.Entities;

namespace AuctionApi.Core.Interfaces;

public interface IAuctionRepository : IRepository<Auction>
{
    Task<IEnumerable<Auction>> GetFilteredAsync(string? search, string? status, CancellationToken ct = default);
    Task<Auction?> GetWithDetailsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Auction>> ListAllAsync(CancellationToken ct = default);
}
