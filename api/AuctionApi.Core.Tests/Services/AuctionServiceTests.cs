using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Entities;
using AuctionApi.Core.Services;
using AuctionApi.Core.Tests.Fakes;
using Xunit;

namespace AuctionApi.Core.Tests.Services;

public class AuctionServiceTests
{
    private readonly InMemoryUnitOfWork _uow = new();
    private readonly FakeCurrentUser _user = new() { UserId = 1, Role = "User" };

    private AuctionService Create() => new(_uow, _user);

    private static DateTime Future(DateTime start) => start.AddDays(7);

    [Fact]
    public async Task GetWithDetailsAsync_with_existing_id_returns_auction()
    {
        var a = new Auction { Id = 1, Title = "X", Description = "d", StartingPrice = 10, StartDate = DateTime.UtcNow, EndDate = Future(DateTime.UtcNow), UserId = 1, IsActive = true };
        await _uow.Auctions.AddAsync(a);

        var fetched = await Create().GetWithDetailsAsync(1);
        Assert.Equal("X", fetched.Title);
    }

    [Fact]
    public async Task GetWithDetailsAsync_with_missing_id_throws_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Create().GetWithDetailsAsync(999));
    }

    [Fact]
    public async Task CreateAsync_with_valid_input_persists_auction()
    {
        var now = DateTime.UtcNow;
        var auction = await Create().CreateAsync("Test", "Description", 100m, now, Future(now));

        Assert.Equal("Test", auction.Title);
        Assert.Equal(1, auction.UserId);
        Assert.True(auction.IsActive);
        Assert.Single(_uow.Auctions.All);
    }

    [Theory]
    [InlineData("",    "Description", 100)]
    [InlineData("Test", "",            100)]
    [InlineData("Test", "Description",   0)]
    [InlineData("Test", "Description",  -1)]
    public async Task CreateAsync_with_invalid_fields_throws_ValidationException(string title, string desc, decimal price)
    {
        var now = DateTime.UtcNow;
        await Assert.ThrowsAsync<ValidationException>(() =>
            Create().CreateAsync(title, desc, price, now, Future(now)));
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(9_999_999.99)]
    public async Task CreateAsync_with_positive_prices_succeeds(decimal price)
    {
        var now = DateTime.UtcNow;
        var a = await Create().CreateAsync("T", "D", price, now, Future(now));
        Assert.Equal(price, a.StartingPrice);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task CreateAsync_with_whitespace_title_throws_ValidationException(string title)
    {
        var now = DateTime.UtcNow;
        await Assert.ThrowsAsync<ValidationException>(() =>
            Create().CreateAsync(title, "D", 10m, now, Future(now)));
    }

    [Fact]
    public async Task CreateAsync_with_end_before_start_throws_ValidationException()
    {
        var now = DateTime.UtcNow;
        await Assert.ThrowsAsync<ValidationException>(() =>
            Create().CreateAsync("T", "D", 10m, Future(now), now));
    }

    [Fact]
    public async Task CreateAsync_with_end_in_the_past_throws_ValidationException()
    {
        var past = DateTime.UtcNow.AddDays(-1);
        await Assert.ThrowsAsync<ValidationException>(() =>
            Create().CreateAsync("T", "D", 10m, past.AddDays(-1), past));
    }

    [Fact]
    public async Task UpdateAsync_as_owner_updates_fields()
    {
        var now = DateTime.UtcNow;
        var a = new Auction { Id = 1, Title = "Old", Description = "d", StartingPrice = 10, StartDate = now, EndDate = Future(now), UserId = _user.UserId, IsActive = true };
        await _uow.Auctions.AddAsync(a);

        await Create().UpdateAsync(1, "New", "new desc", Future(now).AddDays(1));
        var fresh = await Create().GetWithDetailsAsync(1);
        Assert.Equal("New", fresh.Title);
        Assert.Equal("new desc", fresh.Description);
    }

    [Fact]
    public async Task UpdateAsync_as_non_owner_throws_Forbidden()
    {
        var a = new Auction { Id = 1, Title = "Old", Description = "d", StartingPrice = 10, StartDate = DateTime.UtcNow, EndDate = Future(DateTime.UtcNow), UserId = 999, IsActive = true };
        await _uow.Auctions.AddAsync(a);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Create().UpdateAsync(1, "New", "new desc", null));
    }

    [Fact]
    public async Task UpdateAsync_as_admin_overrides_owner_check()
    {
        _user.Role = "Admin";
        var a = new Auction { Id = 1, Title = "Old", Description = "d", StartingPrice = 10, StartDate = DateTime.UtcNow, EndDate = Future(DateTime.UtcNow), UserId = 999, IsActive = true };
        await _uow.Auctions.AddAsync(a);

        await Create().UpdateAsync(1, "New", "new desc", null);
        var fresh = await Create().GetWithDetailsAsync(1);
        Assert.Equal("New", fresh.Title);
    }

    [Fact]
    public async Task UpdateAsync_with_end_in_past_throws_ValidationException()
    {
        var now = DateTime.UtcNow;
        var a = new Auction { Id = 1, Title = "Old", Description = "d", StartingPrice = 10, StartDate = now, EndDate = Future(now), UserId = _user.UserId, IsActive = true };
        await _uow.Auctions.AddAsync(a);

        await Assert.ThrowsAsync<ValidationException>(() =>
            Create().UpdateAsync(1, "New", "new desc", now.AddSeconds(-1)));
    }

    [Fact]
    public async Task DeactivateAsync_flips_IsActive()
    {
        var a = new Auction { Id = 1, Title = "X", Description = "d", StartingPrice = 10, StartDate = DateTime.UtcNow, EndDate = Future(DateTime.UtcNow), IsActive = true };
        await _uow.Auctions.AddAsync(a);

        await Create().DeactivateAsync(1);
        var fresh = await Create().GetWithDetailsAsync(1);
        Assert.False(fresh.IsActive);
    }

    [Fact]
    public async Task ActivateAsync_flips_IsActive_back()
    {
        var a = new Auction { Id = 1, Title = "X", Description = "d", StartingPrice = 10, StartDate = DateTime.UtcNow, EndDate = Future(DateTime.UtcNow), IsActive = false };
        await _uow.Auctions.AddAsync(a);

        await Create().ActivateAsync(1);
        var fresh = await Create().GetWithDetailsAsync(1);
        Assert.True(fresh.IsActive);
    }
}
