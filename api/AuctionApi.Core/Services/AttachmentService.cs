using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Entities;
using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Services;

public class AttachmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStorage _storage;
    private readonly ICurrentUser _currentUser;

    public AttachmentService(IUnitOfWork uow, IFileStorage storage, ICurrentUser currentUser)
    {
        _uow = uow;
        _storage = storage;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<Attachment>> AddToAuctionAsync(int auctionId, IReadOnlyList<UploadedFile> files, CancellationToken ct = default)
    {
        var auction = await _uow.Auctions.GetByIdAsync(auctionId, ct);
        if (auction == null) throw new NotFoundException("Auction", auctionId);
        if (auction.UserId != _currentUser.UserId)
            throw new ForbiddenException("You can only modify your own auctions.");

        var saved = new List<Attachment>();

        foreach (var file in files)
        {
            if (file.Size > _storage.MaxFileSize)
                throw new ValidationException($"File '{file.FileName}' exceeds the {_storage.MaxFileSize / (1024 * 1024)}MB limit.");

            var storedName = await _storage.SaveAsync(file.FileName, file.ContentType, file.Stream, ct);
            if (storedName == null) continue; // extension not allowed

            var attachment = new Attachment
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                StoredFileName = storedName,
                FileSize = file.Size,
                UploadedAt = DateTime.UtcNow,
                AuctionId = auctionId,
            };

            await _uow.Attachments.AddAsync(attachment, ct);
            saved.Add(attachment);
        }

        await _uow.SaveChangesAsync(ct);
        return saved;
    }

    public async Task DeleteAsync(int auctionId, int attachmentId, CancellationToken ct = default)
    {
        var attachment = await _uow.Attachments.GetByIdAsync(attachmentId, ct);
        if (attachment == null || attachment.AuctionId != auctionId)
            throw new NotFoundException("Attachment", attachmentId);

        var auction = await _uow.Auctions.GetByIdAsync(auctionId, ct);
        if (auction == null) throw new NotFoundException("Auction", auctionId);
        if (auction.UserId != _currentUser.UserId)
            throw new ForbiddenException("You can only modify your own auctions.");

        _storage.Delete(attachment.StoredFileName);
        _uow.Attachments.Remove(attachment);
        await _uow.SaveChangesAsync(ct);
    }
}

public record UploadedFile(string FileName, string ContentType, long Size, Stream Stream);
