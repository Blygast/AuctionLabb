using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Services;
using Xunit;

namespace AuctionApi.Core.Tests.Integration;

[Trait("Category", "Integration")]
public class AuctionServiceIntegrationTests : IAsyncLifetime
{
    private readonly LocalDbTestContext _db = new();
    private IntegrationTestBed _bed = null!;

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        _bed = new IntegrationTestBed(_db);
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    private AuctionService NewService() => new(_bed.UowInterface, _bed.CurrentUser);

    private async Task<int> SeedSellerAsync(string email)
    {
        var (seller, _) = await new AuthService(_bed.UowInterface, _bed.Hasher, _bed.Tokens)
            .RegisterAsync("Seller", email, "secret123");
        return seller.Id;
    }

    private static DateTime Future(DateTime start) => start.AddDays(7);

    [Fact]
    public async Task Create_persists_auction_with_all_fields()
    {
        _bed.CurrentUser.UserId = await SeedSellerAsync("seller@example.com");
        var now = DateTime.UtcNow;
        var created = await NewService().CreateAsync("Vase", "A nice vase", 100m, now, Future(now));

        var fromDb = await _bed.UowInterface.Auctions.GetByIdAsync(created.Id);
        Assert.NotNull(fromDb);
        Assert.Equal("Vase", fromDb!.Title);
        Assert.Equal(100m, fromDb.StartingPrice);
        Assert.Equal(_bed.CurrentUser.UserId, fromDb.UserId);
        Assert.True(fromDb.IsActive);
    }

    [Fact]
    public async Task GetFiltered_with_status_open_returns_only_open_auctions()
    {
        _bed.CurrentUser.UserId = await SeedSellerAsync("seller@example.com");
        var now = DateTime.UtcNow;

        var open = await NewService().CreateAsync("Open", "d", 10m, now, Future(now));
        var deactivated = await NewService().CreateAsync("Deact", "d", 10m, now, Future(now));
        await NewService().DeactivateAsync(deactivated.Id);

        // A "closed by date" auction: create it with a valid future end date,
        // then bypass the service to rewind EndDate into the past (the service
        // would otherwise reject "End date must be in the future" at create time).
        var closedByDate = await NewService().CreateAsync("Past", "d", 10m, now, Future(now));
        var tracked = await _bed.UowInterface.Auctions.GetByIdAsync(closedByDate.Id);
        tracked!.EndDate = now.AddDays(-7);
        await _bed.UowInterface.SaveChangesAsync();

        var openAuctions = (await NewService().GetFilteredAsync(null, "open")).ToList();
        var ids = openAuctions.Select(a => a.Id).ToList();

        Assert.Contains(open.Id, ids);
        Assert.DoesNotContain(closedByDate.Id, ids);
        Assert.DoesNotContain(deactivated.Id, ids);
    }

    [Fact]
    public async Task Update_as_owner_persists_changes()
    {
        _bed.CurrentUser.UserId = await SeedSellerAsync("seller@example.com");
        var now = DateTime.UtcNow;
        var created = await NewService().CreateAsync("Old", "d", 10m, now, Future(now));

        await NewService().UpdateAsync(created.Id, "New", "new desc", Future(now).AddDays(1));

        var fromDb = await _bed.UowInterface.Auctions.GetByIdAsync(created.Id);
        Assert.Equal("New", fromDb!.Title);
        Assert.Equal("new desc", fromDb.Description);
    }

    [Fact]
    public async Task Update_as_non_owner_throws_Forbidden_and_does_not_persist()
    {
        var ownerId = await SeedSellerAsync("owner@example.com");
        var otherId = await SeedSellerAsync("other@example.com");
        _bed.CurrentUser.UserId = ownerId;

        var now = DateTime.UtcNow;
        var created = await NewService().CreateAsync("Title", "desc", 10m, now, Future(now));

        _bed.CurrentUser.UserId = otherId;
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            NewService().UpdateAsync(created.Id, "Hacked", "hacked", null));

        var fromDb = await _bed.UowInterface.Auctions.GetByIdAsync(created.Id);
        Assert.Equal("Title", fromDb!.Title);
    }
}
