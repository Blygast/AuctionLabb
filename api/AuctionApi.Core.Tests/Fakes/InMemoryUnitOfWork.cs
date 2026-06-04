using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Tests.Fakes;

/// <summary>
/// Single in-memory unit-of-work holding all four repository fakes. Tests
/// construct one of these and inspect the exposed repo collections directly.
/// </summary>
public class InMemoryUnitOfWork : IUnitOfWork
{
    public InMemoryUserRepository Users { get; } = new();
    public InMemoryAuctionRepository Auctions { get; } = new();
    public InMemoryBidRepository Bids { get; } = new();
    public InMemoryAttachmentRepository Attachments { get; } = new();

    IUserRepository IUnitOfWork.Users => Users;
    IAuctionRepository IUnitOfWork.Auctions => Auctions;
    IBidRepository IUnitOfWork.Bids => Bids;
    IAttachmentRepository IUnitOfWork.Attachments => Attachments;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);
}
