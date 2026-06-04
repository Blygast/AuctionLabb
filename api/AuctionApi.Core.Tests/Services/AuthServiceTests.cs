using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Entities;
using AuctionApi.Core.Services;
using AuctionApi.Core.Tests.Fakes;
using Xunit;

namespace AuctionApi.Core.Tests.Services;

public class AuthServiceTests
{
    private readonly InMemoryUnitOfWork _uow = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeJwtTokenService _tokens = new();

    private AuthService Create() => new(_uow, _hasher, _tokens);

    [Fact]
    public async Task RegisterAsync_with_valid_input_creates_user_and_returns_token()
    {
        var (user, token) = await Create().RegisterAsync("Alice", "alice@example.com", "secret123");

        Assert.Equal("alice@example.com", user.Email);
        Assert.Equal("User", user.Role);
        Assert.True(user.IsActive);
        Assert.StartsWith("fake-token::", token);
        Assert.Single(_uow.Users.All);
    }

    [Fact]
    public async Task RegisterAsync_normalizes_email_to_lowercase()
    {
        var (user, _) = await Create().RegisterAsync("Bob", "  BOB@Example.COM  ", "secret123");
        Assert.Equal("bob@example.com", user.Email);
    }

    [Theory]
    [InlineData("user@example.com",        "user@example.com")]
    [InlineData("USER@EXAMPLE.COM",        "user@example.com")]
    [InlineData("  user@example.com  ",    "user@example.com")]
    [InlineData("User+tag@sub.example.com", "user+tag@sub.example.com")]
    public async Task RegisterAsync_normalizes_various_email_formats(string input, string expected)
    {
        var (user, _) = await Create().RegisterAsync("Eve", input, "secret123");
        Assert.Equal(expected, user.Email);
    }

    [Theory]
    [InlineData("", "a@b.com", "secret123")]
    [InlineData("Alice", "", "secret123")]
    [InlineData("Alice", "a@b.com", "")]
    [InlineData("Alice", "a@b.com", "short")]   // 5 chars
    [InlineData("   ", "a@b.com", "secret123")]
    public async Task RegisterAsync_with_invalid_input_throws_ValidationException(string name, string email, string password)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Create().RegisterAsync(name, email, password));
    }

    [Fact]
    public async Task RegisterAsync_with_duplicate_email_throws_ConflictException()
    {
        var svc = Create();
        await svc.RegisterAsync("Alice", "alice@example.com", "secret123");
        await Assert.ThrowsAsync<ConflictException>(() =>
            svc.RegisterAsync("Alice2", "ALICE@example.com", "secret123"));
    }

    [Fact]
    public async Task LoginAsync_with_correct_credentials_returns_token()
    {
        var svc = Create();
        var (registered, _) = await svc.RegisterAsync("Alice", "alice@example.com", "secret123");

        var (user, token) = await svc.LoginAsync("alice@example.com", "secret123");

        Assert.Equal(registered.Id, user.Id);
        Assert.StartsWith("fake-token::", token);
    }

    [Fact]
    public async Task LoginAsync_with_wrong_password_throws_ValidationException()
    {
        var svc = Create();
        await svc.RegisterAsync("Alice", "alice@example.com", "secret123");
        await Assert.ThrowsAsync<ValidationException>(() =>
            svc.LoginAsync("alice@example.com", "WRONG"));
    }

    [Fact]
    public async Task LoginAsync_with_unknown_email_throws_ValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Create().LoginAsync("nobody@example.com", "secret123"));
    }

    [Fact]
    public async Task LoginAsync_with_deactivated_user_throws_ForbiddenException()
    {
        var svc = Create();
        var (user, _) = await svc.RegisterAsync("Alice", "alice@example.com", "secret123");
        user.IsActive = false;
        _uow.Users.Update(user);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            svc.LoginAsync("alice@example.com", "secret123"));
    }

    [Fact]
    public async Task GetMeAsync_returns_existing_user()
    {
        var svc = Create();
        var (user, _) = await svc.RegisterAsync("Alice", "alice@example.com", "secret123");

        var fetched = await svc.GetMeAsync(user.Id);
        Assert.Equal("alice@example.com", fetched.Email);
    }

    [Fact]
    public async Task GetMeAsync_with_missing_user_throws_ForbiddenException()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() => Create().GetMeAsync(999));
    }

    [Fact]
    public async Task UpdatePasswordAsync_with_correct_current_password_updates_hash()
    {
        var svc = Create();
        var (user, _) = await svc.RegisterAsync("Alice", "alice@example.com", "oldpass1");
        var originalHash = user.PasswordHash;

        await svc.UpdatePasswordAsync(user.Id, "oldpass1", "newpass1");

        Assert.NotEqual(originalHash, user.PasswordHash);
        Assert.True(_hasher.Verify("newpass1", user.PasswordHash));
    }

    [Fact]
    public async Task UpdatePasswordAsync_with_wrong_current_password_throws_ValidationException()
    {
        var svc = Create();
        var (user, _) = await svc.RegisterAsync("Alice", "alice@example.com", "oldpass1");

        await Assert.ThrowsAsync<ValidationException>(() =>
            svc.UpdatePasswordAsync(user.Id, "WRONG", "newpass1"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("short")]   // 5 chars
    public async Task UpdatePasswordAsync_with_short_new_password_throws_ValidationException(string newPwd)
    {
        var svc = Create();
        var (user, _) = await svc.RegisterAsync("Alice", "alice@example.com", "oldpass1");
        await Assert.ThrowsAsync<ValidationException>(() =>
            svc.UpdatePasswordAsync(user.Id, "oldpass1", newPwd));
    }
}
