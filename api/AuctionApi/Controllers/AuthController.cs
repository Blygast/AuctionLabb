using AuctionApi.Common.Mapping;
using AuctionApi.Core.Interfaces;
using AuctionApi.Core.Services;
using AuctionApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly ICurrentUser _currentUser;

    public AuthController(AuthService auth, ICurrentUser currentUser)
    {
        _auth = auth;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto, CancellationToken ct)
    {
        var (user, token) = await _auth.RegisterAsync(dto.Name, dto.Email, dto.Password, ct);
        return Ok(user.ToDto(token));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto, CancellationToken ct)
    {
        var (user, token) = await _auth.LoginAsync(dto.Email, dto.Password, ct);
        return Ok(user.ToDto(token));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> Me(CancellationToken ct)
    {
        var user = await _auth.GetMeAsync(_currentUser.UserId, ct);
        return Ok(user.ToMeDto());
    }

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> UpdatePassword(UpdatePasswordDto dto, CancellationToken ct)
    {
        await _auth.UpdatePasswordAsync(_currentUser.UserId, dto.CurrentPassword, dto.NewPassword, ct);
        return Ok(new { message = "Password updated successfully." });
    }
}
