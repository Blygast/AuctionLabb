using AuctionApi.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuctionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileStorage _storage;

    public FilesController(IFileStorage storage)
    {
        _storage = storage;
    }

    [HttpGet("{fileName}")]
    public IActionResult GetFile(string fileName)
    {
        if (!_storage.Exists(fileName)) return NotFound();

        var path = _storage.GetPath(fileName);
        var bytes = System.IO.File.ReadAllBytes(path);
        var contentType = GetContentType(Path.GetExtension(fileName).ToLowerInvariant());
        return File(bytes, contentType, Path.GetFileName(path));
    }

    private static string GetContentType(string ext) => ext switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".bmp" => "image/bmp",
        ".svg" => "image/svg+xml",
        ".mp4" => "video/mp4",
        ".webm" => "video/webm",
        ".avi" => "video/x-msvideo",
        ".mov" => "video/quicktime",
        ".mkv" => "video/x-matroska",
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".xls" => "application/vnd.ms-excel",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".ppt" => "application/vnd.ms-powerpoint",
        ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        ".txt" => "text/plain",
        ".csv" => "text/csv",
        ".rtf" => "application/rtf",
        ".odt" => "application/vnd.oasis.opendocument.text",
        ".ods" => "application/vnd.oasis.opendocument.spreadsheet",
        ".zip" => "application/zip",
        ".rar" => "application/vnd.rar",
        _ => "application/octet-stream"
    };
}
