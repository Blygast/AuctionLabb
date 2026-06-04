using System.ComponentModel.DataAnnotations;

namespace AuctionApi.DTOs;

public class CreateAuctionDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal StartingPrice { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public List<IFormFile>? Files { get; set; }
}
