using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Entities;
using AuctionApi.Core.Services;
using AuctionApi.Core.Tests.Fakes;
using Xunit;

namespace AuctionApi.Core.Tests.Services;

public class BidServiceTests
{
    private readonly InMemoryUnitOfWork _uow = new();
    private readonly FakeCurrentUser _user = new() { UserId = 1, Role = "User" };

    private BidService Create() => new(_uow, _user);

    private Auction SeedAuction(decimal startingPrice, bool isActive = true, int ownerId = 1, DateTime? endDate = null)
    {
        var a = new Auction
        {
            Title = "X", Description = "d", StartingPrice = startingPrice,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = endDate ?? DateTime.UtcNow.AddDays(7),
            UserId = ownerId, IsActive = isActive,
        };
        _uow.Auctions.AddAsync(a);
        return a;
    }

    [Fact]
    public async Task GetBidsForAuction_with_open_auction_returns_bids()
    {
        var a = SeedAuction(10m);
        await _uow.Bids.AddAsync(new Bid { AuctionId = a.Id, UserId = 2, Amount = 50m, BidDate = DateTime.UtcNow });
        await _uow.Bids.AddAsync(new Bid { AuctionId = a.Id, UserId = 3, Amount = 25m, BidDate = DateTime.UtcNow });

        var bids = await Create().GetBidsForAuctionAsync(a.Id);

        Assert.Equal(2, bids.Count());
    }

    [Fact]
    public async Task GetBidsForAuction_with_closed_auction_returns_empty()
    {
        var a = SeedAuction(10m, endDate: DateTime.UtcNow.AddDays(-1));

        var bids = await Create().GetBidsForAuctionAsync(a.Id);

        Assert.Empty(bids);
    }

    [Fact]
    public async Task GetBidsForAuction_with_missing_auction_throws_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Create().GetBidsForAuctionAsync(999));
    }

    [Fact]
    public async Task PlaceBid_above_highest_succeeds()
    {
        var a = SeedAuction(10m);
        _user.UserId = 2; // not the owner

        var bid = await Create().PlaceBidAsync(a.Id, 50m);

        Assert.Equal(50m, bid.Amount);
        Assert.Equal(_user.UserId, bid.UserId);
        Assert.Single(_uow.Bids.All);
    }

    [Fact]
    public async Task PlaceBid_above_starting_price_when_no_bids_succeeds()
    {
        var a = SeedAuction(10m);
        _user.UserId = 2;

        var bid = await Create().PlaceBidAsync(a.Id, 11m);
        Assert.Equal(11m, bid.Amount);
    }

    [Fact]
    public async Task PlaceBid_at_or_below_highest_throws_ValidationException()
    {
        var a = SeedAuction(10m);
        await _uow.Bids.AddAsync(new Bid { AuctionId = a.Id, UserId = 99, Amount = 100m, BidDate = DateTime.UtcNow });
        _user.UserId = 2;

        await Assert.ThrowsAsync<ValidationException>(() => Create().PlaceBidAsync(a.Id, 100m));
        await Assert.ThrowsAsync<ValidationException>(() => Create().PlaceBidAsync(a.Id, 50m));
    }

    [Theory]
    [InlineData(10.01)]   // just above starting price, no prior bids
    [InlineData(11)]
    [InlineData(100.50)]
    public async Task PlaceBid_above_starting_price_succeeds(decimal amount)
    {
        var a = SeedAuction(10m);
        _user.UserId = 2;

        var bid = await Create().PlaceBidAsync(a.Id, amount);
        Assert.Equal(amount, bid.Amount);
    }

    [Theory]
    [InlineData(50.005)]   // just above 50, should be allowed (decimal precision)
    [InlineData(50.01)]
    [InlineData(99.99)]
    public async Task PlaceBid_just_above_highest_succeeds(decimal amount)
    {
        var a = SeedAuction(10m);
        await _uow.Bids.AddAsync(new Bid { AuctionId = a.Id, UserId = 99, Amount = 50m, BidDate = DateTime.UtcNow });
        _user.UserId = 2;

        var bid = await Create().PlaceBidAsync(a.Id, amount);
        Assert.Equal(amount, bid.Amount);
    }

    [Fact]
    public async Task PlaceBid_on_own_auction_throws_ValidationException()
    {
        var a = SeedAuction(10m);
        // _user.UserId is 1 = auction owner
        await Assert.ThrowsAsync<ValidationException>(() => Create().PlaceBidAsync(a.Id, 50m));
    }

    [Fact]
    public async Task PlaceBid_on_closed_auction_throws_ValidationException()
    {
        var a = SeedAuction(10m, endDate: DateTime.UtcNow.AddDays(-1));
        _user.UserId = 2;
        await Assert.ThrowsAsync<ValidationException>(() => Create().PlaceBidAsync(a.Id, 50m));
    }

    [Fact]
    public async Task PlaceBid_with_zero_or_negative_throws_ValidationException()
    {
        var a = SeedAuction(10m);
        _user.UserId = 2;
        await Assert.ThrowsAsync<ValidationException>(() => Create().PlaceBidAsync(a.Id, 0m));
        await Assert.ThrowsAsync<ValidationException>(() => Create().PlaceBidAsync(a.Id, -1m));
    }

    [Fact]
    public async Task CancelBid_as_latest_owner_succeeds()
    {
        var a = SeedAuction(10m);
        _user.UserId = 2;
        var bid = await Create().PlaceBidAsync(a.Id, 50m);

        await Create().CancelBidAsync(a.Id, bid.Id);

        Assert.Empty(_uow.Bids.All);
    }

    [Fact]
    public async Task CancelBid_when_not_latest_throws_ValidationException()
    {
        var a = SeedAuction(10m);
        var firstUser = 2;
        var secondUser = 3;

        _user.UserId = firstUser;
        var olderBid = await Create().PlaceBidAsync(a.Id, 20m);

        _user.UserId = secondUser;
        var latestBid = await Create().PlaceBidAsync(a.Id, 50m);

        // firstUser tries to cancel their old bid (no longer the latest)
        _user.UserId = firstUser;
        await Assert.ThrowsAsync<ValidationException>(() => Create().CancelBidAsync(a.Id, olderBid.Id));
    }

    [Fact]
    public async Task CancelBid_by_non_owner_throws_Forbidden()
    {
        var a = SeedAuction(10m);
        _user.UserId = 2;
        var bid = await Create().PlaceBidAsync(a.Id, 50m);

        _user.UserId = 3;
        await Assert.ThrowsAsync<ForbiddenException>(() => Create().CancelBidAsync(a.Id, bid.Id));
    }

    [Fact]
    public async Task CancelBid_on_closed_auction_throws_ValidationException()
    {
        var a = SeedAuction(10m);
        _user.UserId = 2;
        var bid = await Create().PlaceBidAsync(a.Id, 50m);

        // close the auction after the bid
        a.EndDate = DateTime.UtcNow.AddDays(-1);
        _uow.Auctions.Update(a);

        await Assert.ThrowsAsync<ValidationException>(() => Create().CancelBidAsync(a.Id, bid.Id));
    }
}
