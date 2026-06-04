using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Tests.Fakes;

public class InMemoryBidRepository : InMemoryRepositoryBase<Bid>, IBidRepository
{
    public Task<IEnumerable<Bid>> GetBidsForAuctionAsync(int auctionId, CancellationToken ct = default) =>
        Task.FromResult<IEnumerable<Bid>>(_items
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.BidDate)
            .ToList());

    public Task<Bid?> GetLatestBidAsync(int auctionId, CancellationToken ct = default) =>
        Task.FromResult(_items
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.BidDate)
            .FirstOrDefault());
}
