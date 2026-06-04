namespace AuctionApi.Core.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(int userId, string name, string email, string role = "User");
}
