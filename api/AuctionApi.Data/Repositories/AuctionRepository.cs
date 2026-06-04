using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuctionApi.Data.Repositories;

public class AuctionRepository : Repository<Auction>, IAuctionRepository
{
    public AuctionRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Auction>> GetFilteredAsync(string? search, string? status, CancellationToken ct = default)
    {
        var query = _set
            .Include(a => a.User)
            .Include(a => a.Bids).ThenInclude(b => b.User)
            .Include(a => a.Attachments)
            .AsNoTracking()
            .AsQueryable();

        query = status switch
        {
            "open" => query.Where(a => a.IsActive && a.EndDate > DateTime.UtcNow),
            "closed" => query.Where(a => !a.IsActive || a.EndDate <= DateTime.UtcNow),
            "all" => query,
            _ => query.Where(a => a.IsActive)
        };

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Title.Contains(search));

        return await query.OrderByDescending(a => a.StartDate).ToListAsync(ct);
    }

    // No AsNoTracking here: AuctionService.UpdateAsync mutates the returned
    // entity and then calls Update() — the entity needs to be tracked so
    // EF's change tracker picks up the changes. The GetFilteredAsync and
    // ListAllAsync paths (read-only) DO use AsNoTracking for perf.
    public async Task<Auction?> GetWithDetailsAsync(int id, CancellationToken ct = default) =>
        await _set
            .Include(a => a.User)
            .Include(a => a.Bids).ThenInclude(b => b.User)
            .Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Auction>> ListAllAsync(CancellationToken ct = default) =>
        await _set
            .Include(a => a.User)
            .Include(a => a.Bids)
            .AsNoTracking()
            .ToListAsync(ct);
}
