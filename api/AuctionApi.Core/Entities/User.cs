namespace AuctionApi.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public List<Auction> Auctions { get; set; } = [];
    public List<Bid> Bids { get; set; } = [];
}
