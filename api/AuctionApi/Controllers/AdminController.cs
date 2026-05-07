using System.Security.Claims;
using AuctionApi.Data;
using AuctionApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("users")]
    public async Task<ActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.Id, u.Name, u.Email, u.Role, u.IsActive,
                AuctionCount = u.Auctions.Count,
                BidCount = u.Bids.Count
            })
            .OrderBy(u => u.Id)
            .ToListAsync();

        return Ok(users);
    }

    [HttpPut("users/{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id == adminId) return BadRequest("You cannot deactivate your own account.");

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = false;
        await _context.SaveChangesAsync();
        return Ok(new { message = $"User '{user.Name}' deactivated." });
    }

    [HttpPut("users/{id}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = $"User '{user.Name}' activated." });
    }

    [HttpGet("auctions")]
    public async Task<ActionResult> GetAllAuctions()
    {
        var auctions = await _context.Auctions
            .Include(a => a.User)
            .Include(a => a.Bids)
            .OrderByDescending(a => a.StartDate)
            .Select(a => new
            {
                a.Id, a.Title, a.IsActive, a.StartDate, a.EndDate,
                a.StartingPrice, UserName = a.User.Name, a.UserId,
                BidCount = a.Bids.Count,
                HighestBid = a.Bids.Any() ? a.Bids.Max(b => b.Amount) : (decimal?)null,
                IsOpen = a.EndDate > DateTime.UtcNow && a.IsActive
            })
            .ToListAsync();

        return Ok(auctions);
    }
}
