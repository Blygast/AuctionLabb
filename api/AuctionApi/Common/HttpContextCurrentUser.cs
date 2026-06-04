using System.Security.Claims;
using AuctionApi.Core.Interfaces;

namespace AuctionApi.Common;

public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public int UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : 0;
        }
    }

    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

    public bool IsAdmin => Role == "Admin";
}
