using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Services;

public class BidService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public BidService(IUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<Bid>> GetBidsForAuctionAsync(int auctionId, CancellationToken ct = default)
    {
        var auction = await _uow.Auctions.GetByIdAsync(auctionId, ct);
        if (auction == null) throw new NotFoundException("Auction", auctionId);
        if (!auction.IsOpen) return Array.Empty<Bid>();
        return await _uow.Bids.GetBidsForAuctionAsync(auctionId, ct);
    }

    public async Task<Bid> PlaceBidAsync(int auctionId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0) throw new ValidationException("Bid amount must be positive.");

        var auction = await _uow.Auctions.GetByIdAsync(auctionId, ct);
        if (auction == null) throw new NotFoundException("Auction", auctionId);
        if (!auction.IsOpen) throw new ValidationException("This auction is closed.");
        if (auction.UserId == _currentUser.UserId)
            throw new ValidationException("You cannot bid on your own auction.");

        var bids = await _uow.Bids.GetBidsForAuctionAsync(auctionId, ct);
        var minimumBid = bids.Any() ? bids.Max(b => b.Amount) : auction.StartingPrice;
        if (amount <= minimumBid)
            throw new ValidationException($"Your bid must be higher than {minimumBid:C}.");

        var bid = new Bid
        {
            Amount = amount,
            BidDate = DateTime.UtcNow,
            UserId = _currentUser.UserId,
            AuctionId = auctionId,
        };

        await _uow.Bids.AddAsync(bid, ct);
        await _uow.SaveChangesAsync(ct);

        bid.User = (await _uow.Users.GetByIdAsync(_currentUser.UserId, ct))!;
        return bid;
    }

    public async Task CancelBidAsync(int auctionId, int bidId, CancellationToken ct = default)
    {
        var auction = await _uow.Auctions.GetByIdAsync(auctionId, ct);
        if (auction == null) throw new NotFoundException("Auction", auctionId);
        if (!auction.IsOpen) throw new ValidationException("Cannot cancel bids on a closed auction.");

        var bid = await _uow.Bids.GetByIdAsync(bidId, ct);
        if (bid == null || bid.AuctionId != auctionId) throw new NotFoundException("Bid", bidId);
        if (bid.UserId != _currentUser.UserId)
            throw new ForbiddenException("You can only cancel your own bids.");

        var latest = await _uow.Bids.GetLatestBidAsync(auctionId, ct);
        if (latest?.Id != bidId)
            throw new ValidationException("You can only cancel your latest bid.");

        _uow.Bids.Remove(bid);
        await _uow.SaveChangesAsync(ct);
    }
}
