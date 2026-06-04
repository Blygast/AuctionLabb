using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Services;
using Xunit;

namespace AuctionApi.Core.Tests.Integration;

[Trait("Category", "Integration")]
public class AdminServiceIntegrationTests : IAsyncLifetime
{
    private readonly LocalDbTestContext _db = new();
    private IntegrationTestBed _bed = null!;

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        _bed = new IntegrationTestBed(_db);
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    private AdminService NewService() => new(_bed.UowInterface, _bed.CurrentUser);

    [Fact]
    public async Task Seeded_admin_user_can_be_queried()
    {
        var admin = await _bed.UowInterface.Users.GetByEmailAsync("admin@auctionlabb.com");

        Assert.NotNull(admin);
        Assert.Equal("Admin", admin!.Role);
    }

    [Fact]
    public async Task ListUsers_returns_all_persisted_users()
    {
        await _bed.SeedUser("Alice", "alice@example.com");
        await _bed.SeedUser("Bob", "bob@example.com");

        var users = await NewService().ListUsersAsync();
        // Migrations seed 3 users + the 2 we added.
        Assert.Contains(users, u => u.Email == "alice@example.com");
        Assert.Contains(users, u => u.Email == "bob@example.com");
    }

    [Fact]
    public async Task DeactivateUser_persists_change()
    {
        await _bed.SeedUser("Alice", "alice@example.com", isActive: true);
        var alice = (await _bed.UowInterface.Users.GetByEmailAsync("alice@example.com"))!;
        _bed.CurrentUser.UserId = 999; // not alice

        await NewService().DeactivateUserAsync(alice.Id);

        var fromDb = await _bed.UowInterface.Users.GetByIdAsync(alice.Id);
        Assert.False(fromDb!.IsActive);
    }

    [Fact]
    public async Task DeactivateUser_for_self_throws_and_does_not_persist()
    {
        var admin = (await _bed.UowInterface.Users.GetByEmailAsync("admin@auctionlabb.com"))!;
        _bed.CurrentUser.UserId = admin.Id;

        await Assert.ThrowsAsync<ValidationException>(() =>
            NewService().DeactivateUserAsync(admin.Id));

        var fromDb = await _bed.UowInterface.Users.GetByIdAsync(admin.Id);
        Assert.True(fromDb!.IsActive);
    }
}
