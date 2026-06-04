namespace AuctionApi.Core.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IAuctionRepository Auctions { get; }
    IBidRepository Bids { get; }
    IAttachmentRepository Attachments { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
