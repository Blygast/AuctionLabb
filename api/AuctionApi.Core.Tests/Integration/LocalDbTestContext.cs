using AuctionApi.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AuctionApi.Core.Tests.Integration;

/// <summary>
/// Builds a fresh AppDbContext pointing at a uniquely-named LocalDB database,
/// so every test class gets a clean schema (applied via EF migrations) and
/// a clean teardown (the database is dropped on Dispose).
///
/// Requires LocalDB to be installed (the same instance the AuctionApi
/// runtime project uses). If you don't have it, filter out the
/// "Integration" category: <c>dotnet test --filter "Category!=Integration"</c>.
/// </summary>
public class LocalDbTestContext : IAsyncLifetime
{
    private readonly string _dbName = $"AuctionDb_Tests_{Guid.NewGuid():N}";
    private DbContextOptions<AppDbContext>? _options;
    private bool _ready;

    public AppDbContext Db { get; private set; } = null!;

    public string ConnectionString =>
        $@"Server=(localdb)\mssqllocaldb;Database={_dbName};Trusted_Connection=True;MultipleActiveResultSets=true";

    public async Task InitializeAsync()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        Db = new AppDbContext(_options);

        // Apply the same migrations the production project uses.
        await Db.Database.MigrateAsync();
        _ready = true;
    }

    public async Task DisposeAsync()
    {
        if (!_ready) return;
        try { await Db.Database.EnsureDeletedAsync(); } catch { /* best-effort cleanup */ }
        await Db.DisposeAsync();
    }
}
