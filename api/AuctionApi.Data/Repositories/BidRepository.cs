using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuctionApi.Data.Repositories;

public class BidRepository : Repository<Bid>, IBidRepository
{
    public BidRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Bid>> GetBidsForAuctionAsync(int auctionId, CancellationToken ct = default) =>
        await _set
            .Include(b => b.User)
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.BidDate)
            .ToListAsync(ct);

    public Task<Bid?> GetLatestBidAsync(int auctionId, CancellationToken ct = default) =>
        _set
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.BidDate)
            .FirstOrDefaultAsync(ct);
}
