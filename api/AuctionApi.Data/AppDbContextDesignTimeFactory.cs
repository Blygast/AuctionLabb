using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuctionApi.Data;

/// <summary>
/// Allows <c>dotnet ef migrations ...</c> to run from the Data project
/// alone — no startup project, no API host, no <c>AddDbContext</c> wiring
/// required. The connection string is only consulted by design-time tools
/// that need a real database (e.g. <c>migrations script</c>); the migrator
/// only uses it to build the EF model, never to connect.
/// </summary>
public class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=AuctionDb_DesignTime;"
                + "Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options;
        return new AppDbContext(options);
    }
}
