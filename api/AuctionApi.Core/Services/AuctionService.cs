using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Services;

public class AuctionService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public AuctionService(IUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public Task<IEnumerable<Auction>> GetFilteredAsync(string? search, string? status, CancellationToken ct = default) =>
        _uow.Auctions.GetFilteredAsync(search, status, ct);

    public async Task<Auction> GetWithDetailsAsync(int id, CancellationToken ct = default)
    {
        var auction = await _uow.Auctions.GetWithDetailsAsync(id, ct);
        if (auction == null) throw new NotFoundException("Auction", id);
        return auction;
    }

    public async Task<Auction> CreateAsync(
        string title, string description, decimal startingPrice,
        DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        ValidateAuctionInput(title, description, startingPrice, startDate, endDate);

        var auction = new Auction
        {
            Title = title.Trim(),
            Description = description.Trim(),
            StartingPrice = startingPrice,
            StartDate = startDate.ToUniversalTime(),
            EndDate = endDate.ToUniversalTime(),
            UserId = _currentUser.UserId,
            IsActive = true,
        };

        await _uow.Auctions.AddAsync(auction, ct);
        await _uow.SaveChangesAsync(ct);

        return (await _uow.Auctions.GetWithDetailsAsync(auction.Id, ct))!;
    }

    public async Task<Auction> UpdateAsync(int id, string title, string description, DateTime? endDate, CancellationToken ct = default)
    {
        var auction = await _uow.Auctions.GetWithDetailsAsync(id, ct);
        if (auction == null) throw new NotFoundException("Auction", id);

        if (auction.UserId != _currentUser.UserId && !_currentUser.IsAdmin)
            throw new ForbiddenException("You can only edit your own auctions.");

        if (string.IsNullOrWhiteSpace(title)) throw new ValidationException("Title is required.");
        if (string.IsNullOrWhiteSpace(description)) throw new ValidationException("Description is required.");

        auction.Title = title.Trim();
        auction.Description = description.Trim();

        if (endDate.HasValue)
        {
            if (endDate.Value <= DateTime.UtcNow)
                throw new ValidationException("End date must be in the future.");
            if (endDate.Value <= auction.StartDate)
                throw new ValidationException("End date must be after start date.");
            auction.EndDate = endDate.Value.ToUniversalTime();
        }

        _uow.Auctions.Update(auction);
        await _uow.SaveChangesAsync(ct);
        return auction;
    }

    public async Task ActivateAsync(int id, CancellationToken ct = default)
    {
        var auction = await _uow.Auctions.GetByIdAsync(id, ct);
        if (auction == null) throw new NotFoundException("Auction", id);
        auction.IsActive = true;
        _uow.Auctions.Update(auction);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(int id, CancellationToken ct = default)
    {
        var auction = await _uow.Auctions.GetByIdAsync(id, ct);
        if (auction == null) throw new NotFoundException("Auction", id);
        auction.IsActive = false;
        _uow.Auctions.Update(auction);
        await _uow.SaveChangesAsync(ct);
    }

    private static void ValidateAuctionInput(string title, string description, decimal startingPrice, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ValidationException("Title is required.");
        if (string.IsNullOrWhiteSpace(description)) throw new ValidationException("Description is required.");
        if (startingPrice <= 0) throw new ValidationException("Starting price must be positive.");
        if (endDate <= startDate) throw new ValidationException("End date must be after start date.");
        if (endDate <= DateTime.UtcNow) throw new ValidationException("End date must be in the future.");
    }
}
