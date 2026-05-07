namespace AuctionApi.DTOs;

public class BidResponseDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime BidDate { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}
