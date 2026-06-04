using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Entities;
using AuctionApi.Core.Services;
using AuctionApi.Core.Tests.Fakes;
using Xunit;

namespace AuctionApi.Core.Tests.Services;

public class AdminServiceTests
{
    private readonly InMemoryUnitOfWork _uow = new();
    private readonly FakeCurrentUser _user = new() { UserId = 1, Role = "Admin" };

    private AdminService Create() => new(_uow, _user);

    private User SeedUser(int id, bool isActive = true)
    {
        var u = new User
        {
            Id = id, Name = $"User{id}", Email = $"u{id}@x.com",
            PasswordHash = "hashed::x", Role = "User", IsActive = isActive,
        };
        _uow.Users.AddAsync(u);
        return u;
    }

    [Fact]
    public async Task ListUsersAsync_returns_all_users()
    {
        SeedUser(1); SeedUser(2);
        var users = await Create().ListUsersAsync();
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task ListAuctionsAsync_returns_all_auctions()
    {
        await _uow.Auctions.AddAsync(new Auction { Id = 1, Title = "A", Description = "d", StartingPrice = 10, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(7), UserId = 1, IsActive = true });
        await _uow.Auctions.AddAsync(new Auction { Id = 2, Title = "B", Description = "d", StartingPrice = 10, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(7), UserId = 1, IsActive = true });
        var auctions = await Create().ListAuctionsAsync();
        Assert.Equal(2, auctions.Count);
    }

    [Fact]
    public async Task ActivateUserAsync_flips_IsActive()
    {
        var u = SeedUser(2, isActive: false);
        var returned = await Create().ActivateUserAsync(2);
        Assert.True(returned.IsActive);
        Assert.True(_uow.Users.All.Single(x => x.Id == 2).IsActive);
    }

    [Fact]
    public async Task ActivateUserAsync_with_missing_id_throws_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Create().ActivateUserAsync(999));
    }

    [Fact]
    public async Task DeactivateUserAsync_flips_IsActive()
    {
        SeedUser(2, isActive: true);
        var returned = await Create().DeactivateUserAsync(2);
        Assert.False(returned.IsActive);
    }

    [Fact]
    public async Task DeactivateUserAsync_for_self_throws_ValidationException()
    {
        // The current user is ID 1 (the admin)
        SeedUser(1, isActive: true);
        await Assert.ThrowsAsync<ValidationException>(() => Create().DeactivateUserAsync(1));
    }

    [Fact]
    public async Task DeactivateUserAsync_with_missing_id_throws_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Create().DeactivateUserAsync(999));
    }
}
