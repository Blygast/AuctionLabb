using AuctionApi.Core.Entities;
using AuctionApi.DTOs;

namespace AuctionApi.Common.Mapping;

public static class AuctionMapper
{
    public static AuctionResponseDto ToDto(this Auction a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Description = a.Description,
        StartingPrice = a.StartingPrice,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        IsOpen = a.EndDate > DateTime.UtcNow && a.IsActive,
        UserId = a.UserId,
        UserName = a.User?.Name ?? string.Empty,
        HighestBid = a.Bids?.Any() == true ? a.Bids.Max(b => b.Amount) : (decimal?)null,
        IsActive = a.IsActive,
        Attachments = a.Attachments?.Select(ToDto).ToList() ?? [],
        Bids = a.Bids?
            .OrderByDescending(b => b.BidDate)
            .Select(ToDto)
            .ToList() ?? []
    };

    public static BidResponseDto ToDto(this Bid b) => new()
    {
        Id = b.Id,
        Amount = b.Amount,
        BidDate = b.BidDate,
        UserId = b.UserId,
        UserName = b.User?.Name ?? string.Empty
    };

    public static AttachmentResponseDto ToDto(this Attachment att) => new()
    {
        Id = att.Id,
        FileName = att.FileName,
        ContentType = att.ContentType,
        FileSize = att.FileSize,
        UploadedAt = att.UploadedAt,
        Url = $"/api/files/{att.StoredFileName}"
    };
}
