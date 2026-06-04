namespace AuctionApi.Core.Entities;

public class Bid
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime BidDate { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int AuctionId { get; set; }
    public Auction Auction { get; set; } = null!;
}
