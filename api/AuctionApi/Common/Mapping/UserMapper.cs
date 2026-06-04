using AuctionApi.Core.Entities;
using AuctionApi.DTOs;

namespace AuctionApi.Common.Mapping;

public static class UserMapper
{
    public static AuthResponseDto ToDto(this User u, string token) => new()
    {
        Token = token,
        UserId = u.Id,
        Name = u.Name,
        Email = u.Email,
        Role = u.Role
    };

    public static AuthResponseDto ToMeDto(this User u) => new()
    {
        Token = string.Empty,
        UserId = u.Id,
        Name = u.Name,
        Email = u.Email,
        Role = u.Role
    };
}
