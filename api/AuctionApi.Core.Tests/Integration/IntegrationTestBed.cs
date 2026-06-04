using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;
using AuctionApi.Core.Tests.Fakes;
using AuctionApi.Data.Repositories;

namespace AuctionApi.Core.Tests.Integration;

/// <summary>
/// Convenience fakes used by integration tests. They share the real database
/// (via LocalDbTestContext) but stub out the pieces we don't want to bring
/// up — password hashing (BCrypt) and JWT signing — to keep tests fast and
/// deterministic. The caller identity is settable per test.
/// </summary>
public class IntegrationTestBed
{
    public LocalDbTestContext Db { get; }
    /// <summary>The concrete UnitOfWork — exposes the real repositories' collections for assertions.</summary>
    public UnitOfWork Uow { get; }
    /// <summary>The IUnitOfWork interface (what production services consume).</summary>
    public IUnitOfWork UowInterface => Uow;
    public FakePasswordHasher Hasher { get; } = new();
    public FakeJwtTokenService Tokens { get; } = new();
    public FakeCurrentUser CurrentUser { get; } = new();

    public IntegrationTestBed(LocalDbTestContext db)
    {
        Db = db;
        Uow = new UnitOfWork(db.Db);
    }

    public async Task SeedUser(string name, string email, string role = "User", bool isActive = true)
    {
        await Uow.Users.AddAsync(new User
        {
            Name = name, Email = email.ToLowerInvariant(),
            PasswordHash = Hasher.Hash("password123"),
            Role = role, IsActive = isActive,
        });
        await Uow.SaveChangesAsync();
    }
}
