using System.ComponentModel.DataAnnotations;

namespace AuctionApi.DTOs;

public class UpdateAuctionDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    public DateTime? EndDate { get; set; }
}
