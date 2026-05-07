namespace AuctionApi.DTOs;

public class CreateAuctionDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<IFormFile>? Files { get; set; }
}
