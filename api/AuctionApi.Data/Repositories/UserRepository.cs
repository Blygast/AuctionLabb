using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuctionApi.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public Task<User?> GetByEmailAsync(string email) =>
        _set.FirstOrDefaultAsync(u => u.Email == email);

    public Task<bool> EmailExistsAsync(string email) =>
        _set.AnyAsync(u => u.Email == email);

    public async Task<IReadOnlyList<User>> ListAllAsync(CancellationToken ct = default) =>
        await _set.AsNoTracking().ToListAsync(ct);
}
