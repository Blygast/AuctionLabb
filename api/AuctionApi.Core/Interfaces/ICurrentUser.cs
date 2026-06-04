namespace AuctionApi.Core.Interfaces;

public interface ICurrentUser
{
    int UserId { get; }
    string? Role { get; }
    bool IsAdmin { get; }
}
