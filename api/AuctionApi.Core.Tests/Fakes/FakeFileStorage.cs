using AuctionApi.Core.Interfaces;

namespace AuctionApi.Core.Tests.Fakes;

public class FakeFileStorage : IFileStorage
{
    public HashSet<string> Allowed { get; } = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".png", ".pdf" };
    public HashSet<string> Saved { get; } = new();
    public HashSet<string> Deleted { get; } = new();
    public bool ShouldReject { get; set; } = false;

    public long MaxFileSize => 1024 * 1024; // 1 MB for tests

    public bool IsAllowedExtension(string fileName) =>
        Allowed.Contains(Path.GetExtension(fileName));

    public Task<string?> SaveAsync(string originalFileName, string contentType, Stream stream, CancellationToken ct = default)
    {
        if (ShouldReject || !IsAllowedExtension(originalFileName)) return Task.FromResult<string?>(null);
        var stored = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
        Saved.Add(stored);
        return Task.FromResult<string?>(stored);
    }

    public string GetPath(string storedFileName) => Path.Combine("/tmp", storedFileName);
    public bool Exists(string storedFileName) => Saved.Contains(storedFileName);
    public void Delete(string storedFileName) => Deleted.Add(storedFileName);
}
