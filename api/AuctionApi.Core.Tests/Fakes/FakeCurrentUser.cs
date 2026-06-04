using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Tests.Fakes;

/// <summary>Test double that lets each test set the caller identity explicitly.</summary>
public class FakeCurrentUser : ICurrentUser
{
    public int UserId { get; set; }
    public string? Role { get; set; }
    public bool IsAdmin => Role == "Admin";
}
