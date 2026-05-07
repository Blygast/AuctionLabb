namespace AuctionApi.DTOs;

public class AuctionResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsOpen { get; set; }
    public bool IsActive { get; set; } = true;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal? HighestBid { get; set; }
    public List<AttachmentResponseDto> Attachments { get; set; } = [];
    public List<BidResponseDto> Bids { get; set; } = [];
    public BidResponseDto? WinningBid { get; set; }
}
