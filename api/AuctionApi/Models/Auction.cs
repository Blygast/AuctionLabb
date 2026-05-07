namespace AuctionApi.Models;

public class Auction
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public List<Bid> Bids { get; set; } = [];
    public List<Attachment> Attachments { get; set; } = [];
    public bool IsOpen => EndDate > DateTime.UtcNow && IsActive;
}
