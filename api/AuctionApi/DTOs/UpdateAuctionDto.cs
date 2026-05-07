namespace AuctionApi.DTOs;

public class UpdateAuctionDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }
}
