using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Services;

public class AdminService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public AdminService(IUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<User>> ListUsersAsync(CancellationToken ct = default) => _uow.Users.ListAllAsync(ct);
    public Task<IReadOnlyList<Auction>> ListAuctionsAsync(CancellationToken ct = default) => _uow.Auctions.ListAllAsync(ct);

    public async Task<User> ActivateUserAsync(int id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct);
        if (user == null) throw new NotFoundException("User", id);
        user.IsActive = true;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        return user;
    }

    public async Task<User> DeactivateUserAsync(int id, CancellationToken ct = default)
    {
        if (id == _currentUser.UserId)
            throw new ValidationException("You cannot deactivate your own account.");

        var user = await _uow.Users.GetByIdAsync(id, ct);
        if (user == null) throw new NotFoundException("User", id);
        user.IsActive = false;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        return user;
    }
}
