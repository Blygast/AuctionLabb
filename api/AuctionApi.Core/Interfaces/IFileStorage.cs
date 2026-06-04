namespace AuctionApi.Core.Interfaces;

public interface IFileStorage
{
    Task<string?> SaveAsync(string originalFileName, string contentType, Stream stream, CancellationToken ct = default);

    string GetPath(string storedFileName);

    bool Exists(string storedFileName);

    void Delete(string storedFileName);

    bool IsAllowedExtension(string fileName);

    long MaxFileSize { get; }
}
