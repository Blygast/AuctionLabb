using AuctionApi.Core.Interfaces;

namespace AuctionApi.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
        Users = new UserRepository(db);
        Auctions = new AuctionRepository(db);
        Bids = new BidRepository(db);
        Attachments = new AttachmentRepository(db);
    }

    public IUserRepository Users { get; }
    public IAuctionRepository Auctions { get; }
    public IBidRepository Bids { get; }
    public IAttachmentRepository Attachments { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
