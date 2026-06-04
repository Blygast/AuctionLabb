using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Services;
using Xunit;

namespace AuctionApi.Core.Tests.Integration;

[Trait("Category", "Integration")]
public class BidServiceIntegrationTests : IAsyncLifetime
{
    private readonly LocalDbTestContext _db = new();
    private IntegrationTestBed _bed = null!;

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        _bed = new IntegrationTestBed(_db);
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    private BidService NewService() => new(_bed.UowInterface, _bed.CurrentUser);

    private async Task<(int sellerId, int bidderId, int auctionId)> SeedAsync()
    {
        var auth = new AuthService(_bed.UowInterface, _bed.Hasher, _bed.Tokens);
        var (seller, _) = await auth.RegisterAsync("Seller", "seller@example.com", "secret123");
        var (bidder, _) = await auth.RegisterAsync("Bidder", "bidder@example.com", "secret123");
        _bed.CurrentUser.UserId = seller.Id;
        var auction = await new AuctionService(_bed.UowInterface, _bed.CurrentUser)
            .CreateAsync("A", "d", 10m, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(7));
        return (seller.Id, bidder.Id, auction.Id);
    }

    [Fact]
    public async Task PlaceBid_persists_and_appears_in_GetBidsForAuction()
    {
        var (_, bidderId, auctionId) = await SeedAsync();
        _bed.CurrentUser.UserId = bidderId;

        var bid = await NewService().PlaceBidAsync(auctionId, 50m);

        var bids = (await NewService().GetBidsForAuctionAsync(auctionId)).ToList();
        Assert.Single(bids);
        Assert.Equal(bid.Id, bids[0].Id);
        Assert.Equal(50m, bids[0].Amount);
    }

    [Fact]
    public async Task PlaceBid_three_bids_returns_in_descending_date_order()
    {
        var (_, bidderId, auctionId) = await SeedAsync();
        _bed.CurrentUser.UserId = bidderId;

        await NewService().PlaceBidAsync(auctionId, 50m);
        await Task.Delay(10);
        await NewService().PlaceBidAsync(auctionId, 75m);
        await Task.Delay(10);
        await NewService().PlaceBidAsync(auctionId, 100m);

        var bids = (await NewService().GetBidsForAuctionAsync(auctionId)).ToList();
        Assert.Equal(new[] { 100m, 75m, 50m }, bids.Select(b => b.Amount).ToArray());
    }

    [Fact]
    public async Task PlaceBid_below_highest_throws_and_does_not_persist()
    {
        var (_, bidderId, auctionId) = await SeedAsync();
        _bed.CurrentUser.UserId = bidderId;

        await NewService().PlaceBidAsync(auctionId, 100m);
        var countBefore = (await _bed.UowInterface.Bids.GetBidsForAuctionAsync(auctionId)).Count();

        await Assert.ThrowsAsync<ValidationException>(() =>
            NewService().PlaceBidAsync(auctionId, 50m));

        Assert.Equal(countBefore, (await _bed.UowInterface.Bids.GetBidsForAuctionAsync(auctionId)).Count());
    }

    [Fact]
    public async Task CancelBid_removes_row()
    {
        var (_, bidderId, auctionId) = await SeedAsync();
        _bed.CurrentUser.UserId = bidderId;
        var bid = await NewService().PlaceBidAsync(auctionId, 50m);

        await NewService().CancelBidAsync(auctionId, bid.Id);

        Assert.Empty(await _bed.UowInterface.Bids.GetBidsForAuctionAsync(auctionId));
    }
}
