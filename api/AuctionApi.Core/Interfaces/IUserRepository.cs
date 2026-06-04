using AuctionApi.Core.Entities;

namespace AuctionApi.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<IReadOnlyList<User>> ListAllAsync(CancellationToken ct = default);
}
