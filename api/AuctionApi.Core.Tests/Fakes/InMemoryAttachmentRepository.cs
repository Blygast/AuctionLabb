using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Tests.Fakes;

public class InMemoryAttachmentRepository : InMemoryRepositoryBase<Attachment>, IAttachmentRepository
{
}
