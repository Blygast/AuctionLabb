using AuctionApi.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _admin;

    public AdminController(AdminService admin)
    {
        _admin = admin;
    }

    [HttpGet("users")]
    public async Task<ActionResult> GetUsers(CancellationToken ct)
    {
        var users = await _admin.ListUsersAsync(ct);
        return Ok(users.Select(u => new
        {
            u.Id, u.Name, u.Email, u.Role, u.IsActive,
            AuctionCount = u.Auctions.Count,
            BidCount = u.Bids.Count,
        }));
    }

    [HttpPut("users/{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id, CancellationToken ct)
    {
        var user = await _admin.DeactivateUserAsync(id, ct);
        return Ok(new { message = $"User '{user.Name}' deactivated." });
    }

    [HttpPut("users/{id:int}/activate")]
    public async Task<IActionResult> ActivateUser(int id, CancellationToken ct)
    {
        var user = await _admin.ActivateUserAsync(id, ct);
        return Ok(new { message = $"User '{user.Name}' activated." });
    }

    [HttpGet("auctions")]
    public async Task<ActionResult> GetAllAuctions(CancellationToken ct)
    {
        var auctions = await _admin.ListAuctionsAsync(ct);
        return Ok(auctions.Select(a => new
        {
            a.Id, a.Title, a.IsActive, a.StartDate, a.EndDate,
            a.StartingPrice, UserName = a.User.Name, a.UserId,
            BidCount = a.Bids.Count,
            HighestBid = a.Bids.Any() ? a.Bids.Max(b => b.Amount) : (decimal?)null,
            IsOpen = a.EndDate > DateTime.UtcNow && a.IsActive,
        }));
    }
}
