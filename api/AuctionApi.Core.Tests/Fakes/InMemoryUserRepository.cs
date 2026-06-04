using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Tests.Fakes;

public class InMemoryUserRepository : InMemoryRepositoryBase<User>, IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) =>
        FirstOrNull(u => u.Email == email);

    public Task<bool> EmailExistsAsync(string email) =>
        Task.FromResult(_items.Any(u => u.Email == email));

    public Task<IReadOnlyList<User>> ListAllAsync(CancellationToken ct = default) =>
        Task.FromResult((IReadOnlyList<User>)_items);
}
