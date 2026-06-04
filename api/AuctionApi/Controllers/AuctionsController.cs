using System.Security.Claims;
using AuctionApi.Data;
using AuctionApi.DTOs;
using AuctionApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg",
        ".mp4", ".webm", ".avi", ".mov", ".mkv",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".rtf", ".odt", ".ods", ".zip", ".rar"
    };
    private const long MaxFileSize = 50 * 1024 * 1024;

    public AuctionsController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    private bool IsAdmin => User.FindFirstValue(ClaimTypes.Role) == "Admin";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuctionResponseDto>>> GetAuctions(
        [FromQuery] string? search,
        [FromQuery] string? status)
    {
        var query = _context.Auctions
            .Include(a => a.User)
            .Include(a => a.Bids).ThenInclude(b => b.User)
            .Include(a => a.Attachments)
            .AsQueryable();

        query = status switch
        {
            "open" => query.Where(a => a.IsActive && a.EndDate > DateTime.UtcNow),
            "closed" => query.Where(a => !a.IsActive || a.EndDate <= DateTime.UtcNow),
            "all" => query,
            _ => query.Where(a => a.IsActive)
        };

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Title.Contains(search));

        var auctions = await query
            .OrderByDescending(a => a.StartDate)
            .Select(a => MapToResponse(a))
            .ToListAsync();

        return Ok(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionResponseDto>> GetAuction(int id)
    {
        var auction = await _context.Auctions
            .Include(a => a.User)
            .Include(a => a.Bids).ThenInclude(b => b.User)
            .Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null) return NotFound();

        var dto = MapToResponse(auction);

        if (!auction.IsOpen)
        {
            dto.Bids = [];
            dto.WinningBid = auction.Bids.Any()
                ? auction.Bids.OrderByDescending(b => b.Amount).Select(b => new BidResponseDto
                {
                    Id = b.Id, Amount = b.Amount, BidDate = b.BidDate,
                    UserId = b.UserId, UserName = b.User.Name
                }).First()
                : null;
        }

        return Ok(dto);
    }

    [HttpPost]
    [Authorize]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<ActionResult<AuctionResponseDto>> CreateAuction([FromForm] CreateAuctionDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (dto.EndDate <= dto.StartDate)
            return BadRequest("End date must be after start date.");

        if (dto.EndDate <= DateTime.UtcNow)
            return BadRequest("End date must be in the future.");

        var auction = new Auction
        {
            Title = dto.Title,
            Description = dto.Description,
            StartingPrice = dto.StartingPrice,
            StartDate = dto.StartDate.ToUniversalTime(),
            EndDate = dto.EndDate.ToUniversalTime(),
            UserId = userId,
            IsActive = true
        };

        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();

        if (dto.Files != null && dto.Files.Count > 0)
            await SaveAttachments(auction.Id, dto.Files);

        var user = await _context.Users.FindAsync(userId);
        var attachments = await _context.Attachments.Where(a => a.AuctionId == auction.Id).ToListAsync();

        return CreatedAtAction(nameof(GetAuction), new { id = auction.Id }, new AuctionResponseDto
        {
            Id = auction.Id, Title = auction.Title, Description = auction.Description,
            StartingPrice = auction.StartingPrice, StartDate = auction.StartDate,
            EndDate = auction.EndDate, IsOpen = true, UserId = auction.UserId,
            UserName = user!.Name, HighestBid = null,
            Attachments = attachments.Select(MapAttachment).ToList()
        });
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<AuctionResponseDto>> UpdateAuction(int id, UpdateAuctionDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var auction = await _context.Auctions
            .Include(a => a.User).Include(a => a.Bids).ThenInclude(b => b.User).Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null) return NotFound();
        if (auction.UserId != userId && !IsAdmin) return Forbid();

        auction.Title = dto.Title;
        auction.Description = dto.Description;

        if (dto.EndDate.HasValue)
        {
            if (dto.EndDate.Value <= DateTime.UtcNow)
                return BadRequest("End date must be in the future.");
            if (dto.EndDate.Value <= auction.StartDate)
                return BadRequest("End date must be after start date.");
            auction.EndDate = dto.EndDate.Value.ToUniversalTime();
        }

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(auction));
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateAuction(int id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null) return NotFound();

        auction.IsActive = false;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Auction deactivated." });
    }

    [HttpPut("{id}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateAuction(int id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null) return NotFound();

        auction.IsActive = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Auction activated." });
    }

    [HttpPost("{id}/attachments")]
    [Authorize]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<ActionResult<IEnumerable<AttachmentResponseDto>>> UploadAttachments(int id, List<IFormFile> files)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var auction = await _context.Auctions.FindAsync(id);

        if (auction == null) return NotFound("Auction not found.");
        if (auction.UserId != userId) return Forbid();

        var attachments = await SaveAttachments(auction.Id, files);
        return Ok(attachments.Select(MapAttachment));
    }

    [HttpDelete("{auctionId}/attachments/{attachmentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteAttachment(int auctionId, int attachmentId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var attachment = await _context.Attachments.FindAsync(attachmentId);

        if (attachment == null || attachment.AuctionId != auctionId) return NotFound();

        var auction = await _context.Auctions.FindAsync(auctionId);
        if (auction?.UserId != userId) return Forbid();

        var filePath = Path.Combine(_env.ContentRootPath, "Uploads", attachment.StoredFileName);
        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/bids")]
    public async Task<ActionResult<IEnumerable<BidResponseDto>>> GetBidsForAuction(int id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null) return NotFound();

        if (!auction.IsOpen)
            return Ok(new List<BidResponseDto>());

        var bids = await _context.Bids
            .Include(b => b.User)
            .Where(b => b.AuctionId == id)
            .OrderByDescending(b => b.BidDate)
            .Select(b => new BidResponseDto
            {
                Id = b.Id, Amount = b.Amount, BidDate = b.BidDate,
                UserId = b.UserId, UserName = b.User.Name
            })
            .ToListAsync();

        return Ok(bids);
    }

    [HttpPost("{id}/bids")]
    [Authorize]
    public async Task<ActionResult<BidResponseDto>> PlaceBid(int id, CreateBidDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var auction = await _context.Auctions.Include(a => a.Bids).FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null) return NotFound("Auction not found.");
        if (!auction.IsOpen) return BadRequest("This auction is closed.");
        if (auction.UserId == userId) return BadRequest("You cannot bid on your own auction.");

        var minimumBid = auction.Bids.Any() ? auction.Bids.Max(b => b.Amount) : auction.StartingPrice;
        if (dto.Amount <= minimumBid)
            return BadRequest($"Your bid must be higher than {minimumBid:C}.");

        var bid = new Bid
        {
            Amount = dto.Amount, BidDate = DateTime.UtcNow,
            UserId = userId, AuctionId = id
        };

        _context.Bids.Add(bid);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        return Ok(new BidResponseDto
        {
            Id = bid.Id, Amount = bid.Amount, BidDate = bid.BidDate,
            UserId = bid.UserId, UserName = user!.Name
        });
    }

    [HttpDelete("{auctionId}/bids/{bidId}")]
    [Authorize]
    public async Task<IActionResult> CancelBid(int auctionId, int bidId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var auction = await _context.Auctions.FindAsync(auctionId);
        if (auction == null) return NotFound("Auction not found.");
        if (!auction.IsOpen) return BadRequest("Cannot cancel bids on a closed auction.");

        var bid = await _context.Bids.FindAsync(bidId);
        if (bid == null || bid.AuctionId != auctionId) return NotFound("Bid not found.");
        if (bid.UserId != userId) return Forbid("You can only cancel your own bids.");

        var latestBid = await _context.Bids
            .Where(b => b.AuctionId == auctionId)
            .OrderByDescending(b => b.BidDate)
            .FirstOrDefaultAsync();

        if (latestBid?.Id != bidId)
            return BadRequest("You can only cancel your latest bid.");

        _context.Bids.Remove(bid);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Bid cancelled." });
    }

    private async Task<List<Attachment>> SaveAttachments(int auctionId, List<IFormFile> files)
    {
        var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads");
        Directory.CreateDirectory(uploadsDir);
        var attachments = new List<Attachment>();

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(ext)) continue;

            var storedName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, storedName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var attachment = new Attachment
            {
                FileName = file.FileName, ContentType = file.ContentType,
                StoredFileName = storedName, FileSize = file.Length,
                UploadedAt = DateTime.UtcNow, AuctionId = auctionId
            };

            _context.Attachments.Add(attachment);
            attachments.Add(attachment);
        }

        await _context.SaveChangesAsync();
        return attachments;
    }

    private static AuctionResponseDto MapToResponse(Auction a) => new()
    {
        Id = a.Id, Title = a.Title, Description = a.Description,
        StartingPrice = a.StartingPrice, StartDate = a.StartDate,
        EndDate = a.EndDate, IsOpen = a.EndDate > DateTime.UtcNow && a.IsActive,
        UserId = a.UserId, UserName = a.User.Name,
        HighestBid = a.Bids.Any() ? a.Bids.Max(b => b.Amount) : (decimal?)null,
        IsActive = a.IsActive,
        Attachments = a.Attachments.Select(MapAttachment).ToList(),
        Bids = a.Bids.OrderByDescending(b => b.BidDate)
            .Select(b => new BidResponseDto
            {
                Id = b.Id, Amount = b.Amount, BidDate = b.BidDate,
                UserId = b.UserId, UserName = b.User.Name
            }).ToList()
    };

    private static AttachmentResponseDto MapAttachment(Attachment att) => new()
    {
        Id = att.Id, FileName = att.FileName, ContentType = att.ContentType,
        FileSize = att.FileSize, UploadedAt = att.UploadedAt,
        Url = $"/api/files/{att.StoredFileName}"
    };
}
