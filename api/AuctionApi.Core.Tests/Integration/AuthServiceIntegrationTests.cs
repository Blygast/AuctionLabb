using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Services;
using Xunit;

namespace AuctionApi.Core.Tests.Integration;

[Trait("Category", "Integration")]
public class AuthServiceIntegrationTests : IAsyncLifetime
{
    private readonly LocalDbTestContext _db = new();
    private IntegrationTestBed _bed = null!;

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        _bed = new IntegrationTestBed(_db);
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    private AuthService NewService() => new(_bed.UowInterface, _bed.Hasher, _bed.Tokens);

    [Fact]
    public async Task Register_persists_user_in_database()
    {
        var (user, token) = await NewService().RegisterAsync("Alice", "alice@example.com", "secret123");

        var fromDb = await _bed.UowInterface.Users.GetByEmailAsync("alice@example.com");

        Assert.NotNull(fromDb);
        Assert.Equal(user.Id, fromDb!.Id);
        Assert.Equal("alice@example.com", fromDb.Email);
        Assert.True(_bed.Hasher.Verify("secret123", fromDb.PasswordHash));
        Assert.StartsWith("fake-token::", token);
    }

    [Fact]
    public async Task Register_with_duplicate_email_throws_and_does_not_write()
    {
        var svc = NewService();
        await svc.RegisterAsync("Alice", "alice@example.com", "secret123");
        var countBefore = (await _bed.UowInterface.Users.ListAllAsync()).Count;

        await Assert.ThrowsAsync<ConflictException>(() =>
            svc.RegisterAsync("Alice2", "ALICE@example.com", "secret123"));

        Assert.Equal(countBefore, (await _bed.UowInterface.Users.ListAllAsync()).Count);
    }

    [Fact]
    public async Task Login_with_correct_credentials_returns_token_for_persisted_user()
    {
        await NewService().RegisterAsync("Bob", "bob@example.com", "secret123");

        var (user, token) = await NewService().LoginAsync("bob@example.com", "secret123");

        Assert.True(user.Id > 0);
        Assert.StartsWith("fake-token::", token);
    }

    [Fact]
    public async Task Login_with_wrong_password_throws()
    {
        await NewService().RegisterAsync("Bob", "bob@example.com", "secret123");
        await Assert.ThrowsAsync<ValidationException>(() =>
            NewService().LoginAsync("bob@example.com", "WRONG"));
    }

    [Fact]
    public async Task UpdatePassword_persists_new_hash()
    {
        var (user, _) = await NewService().RegisterAsync("Bob", "bob@example.com", "oldpass1");

        await NewService().UpdatePasswordAsync(user.Id, "oldpass1", "newpass1");

        var fromDb = await _bed.UowInterface.Users.GetByIdAsync(user.Id);
        Assert.True(_bed.Hasher.Verify("newpass1", fromDb!.PasswordHash));
    }
}
