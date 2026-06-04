using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuctionApi.Data.Repositories;

public class AttachmentRepository : Repository<Attachment>, IAttachmentRepository
{
    public AttachmentRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Attachment>> GetForAuctionAsync(int auctionId) =>
        await _set.Where(a => a.AuctionId == auctionId).ToListAsync();
}
