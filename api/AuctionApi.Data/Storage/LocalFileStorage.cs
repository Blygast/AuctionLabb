using AuctionApi.Core.Interfaces;

namespace AuctionApi.Data.Storage;

public class LocalFileStorage : IFileStorage
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg",
        ".mp4", ".webm", ".avi", ".mov", ".mkv",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".rtf", ".odt", ".ods", ".zip", ".rar"
    };

    private readonly string _root;

    public LocalFileStorage(string rootPath)
    {
        _root = rootPath;
        Directory.CreateDirectory(_root);
    }

    public long MaxFileSize => 50L * 1024 * 1024; // 50 MB

    public bool IsAllowedExtension(string fileName) =>
        AllowedExtensions.Contains(Path.GetExtension(fileName));

    public async Task<string?> SaveAsync(string originalFileName, string contentType, Stream stream, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(originalFileName);
        if (!AllowedExtensions.Contains(ext)) return null;

        var storedName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(_root, storedName);

        await using (var fs = new FileStream(filePath, FileMode.Create))
            await stream.CopyToAsync(fs, ct);

        return storedName;
    }

    public string GetPath(string storedFileName) => Path.Combine(_root, storedFileName);
    public bool Exists(string storedFileName) => File.Exists(GetPath(storedFileName));
    public void Delete(string storedFileName)
    {
        var path = GetPath(storedFileName);
        if (File.Exists(path)) File.Delete(path);
    }
}
