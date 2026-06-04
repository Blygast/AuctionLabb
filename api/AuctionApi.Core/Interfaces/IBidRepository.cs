using AuctionApi.Core.Entities;

namespace AuctionApi.Core.Interfaces;

public interface IBidRepository : IRepository<Bid>
{
    Task<IEnumerable<Bid>> GetBidsForAuctionAsync(int auctionId, CancellationToken ct = default);
    Task<Bid?> GetLatestBidAsync(int auctionId, CancellationToken ct = default);
}
