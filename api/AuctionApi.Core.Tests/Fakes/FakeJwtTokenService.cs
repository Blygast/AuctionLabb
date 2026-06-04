using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Tests.Fakes;

public class FakeJwtTokenService : IJwtTokenService
{
    public string GenerateToken(int userId, string name, string email, string role = "User")
        => $"fake-token::{userId}::{role}";
}
