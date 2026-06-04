using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Tests.Fakes;

/// <summary>
/// Deterministic stand-in for BCrypt. Stores passwords as "hashed::{plaintext}"
/// so tests can both produce and verify hashes without running real crypto.
/// </summary>
public class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hashed::{password}";

    public bool Verify(string password, string hash) => hash == $"hashed::{password}";
}
