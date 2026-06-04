using AuctionApi.Common.Mapping;
using AuctionApi.Core.Services;
using AuctionApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionService _auctions;
    private readonly BidService _bids;
    private readonly AttachmentService _attachments;

    public AuctionsController(AuctionService auctions, BidService bids, AttachmentService attachments)
    {
        _auctions = auctions;
        _bids = bids;
        _attachments = attachments;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuctionResponseDto>>> GetAuctions(
        [FromQuery] string? search,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var auctions = await _auctions.GetFilteredAsync(search, status, ct);
        return Ok(auctions.Select(a => a.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuctionResponseDto>> GetAuction(int id, CancellationToken ct)
    {
        var auction = await _auctions.GetWithDetailsAsync(id, ct);
        var dto = auction.ToDto();

        if (!auction.IsOpen)
        {
            dto.Bids = [];
            dto.WinningBid = auction.Bids?.Any() == true
                ? auction.Bids.OrderByDescending(b => b.Amount).First().ToDto()
                : null;
        }

        return Ok(dto);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<AuctionResponseDto>> CreateAuction([FromForm] CreateAuctionDto dto, CancellationToken ct)
    {
        var auction = await _auctions.CreateAsync(
            dto.Title, dto.Description, dto.StartingPrice, dto.StartDate, dto.EndDate, ct);

        if (dto.Files != null && dto.Files.Count > 0)
        {
            var uploaded = dto.Files
                .Select(f => new UploadedFile(f.FileName, f.ContentType, f.Length, f.OpenReadStream()))
                .ToList();
            await _attachments.AddToAuctionAsync(auction.Id, uploaded, ct);
        }

        var fresh = await _auctions.GetWithDetailsAsync(auction.Id, ct);
        var response = fresh!.ToDto();
        response.Bids = []; // just created, no bids yet
        return CreatedAtAction(nameof(GetAuction), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<AuctionResponseDto>> UpdateAuction(int id, UpdateAuctionDto dto, CancellationToken ct)
    {
        var auction = await _auctions.UpdateAsync(id, dto.Title, dto.Description, dto.EndDate, ct);
        return Ok(auction.ToDto());
    }

    [HttpPut("{id:int}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateAuction(int id, CancellationToken ct)
    {
        await _auctions.DeactivateAsync(id, ct);
        return Ok(new { message = "Auction deactivated." });
    }

    [HttpPut("{id:int}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateAuction(int id, CancellationToken ct)
    {
        await _auctions.ActivateAsync(id, ct);
        return Ok(new { message = "Auction activated." });
    }

    [HttpPost("{id:int}/attachments")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AttachmentResponseDto>>> UploadAttachments(
        int id, List<IFormFile> files, CancellationToken ct)
    {
        var uploaded = files
            .Select(f => new UploadedFile(f.FileName, f.ContentType, f.Length, f.OpenReadStream()))
            .ToList();
        var attachments = await _attachments.AddToAuctionAsync(id, uploaded, ct);
        return Ok(attachments.Select(a => a.ToDto()));
    }

    [HttpDelete("{auctionId:int}/attachments/{attachmentId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteAttachment(int auctionId, int attachmentId, CancellationToken ct)
    {
        await _attachments.DeleteAsync(auctionId, attachmentId, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/bids")]
    public async Task<ActionResult<IEnumerable<BidResponseDto>>> GetBidsForAuction(int id, CancellationToken ct)
    {
        var bids = await _bids.GetBidsForAuctionAsync(id, ct);
        return Ok(bids.Select(b => b.ToDto()));
    }

    [HttpPost("{id:int}/bids")]
    [Authorize]
    public async Task<ActionResult<BidResponseDto>> PlaceBid(int id, CreateBidDto dto, CancellationToken ct)
    {
        var bid = await _bids.PlaceBidAsync(id, dto.Amount, ct);
        return Ok(bid.ToDto());
    }

    [HttpDelete("{auctionId:int}/bids/{bidId:int}")]
    [Authorize]
    public async Task<IActionResult> CancelBid(int auctionId, int bidId, CancellationToken ct)
    {
        await _bids.CancelBidAsync(auctionId, bidId, ct);
        return Ok(new { message = "Bid cancelled." });
    }
}
