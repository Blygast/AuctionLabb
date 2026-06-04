using AuctionApi.Core.Common.Exceptions;
using AuctionApi.Core.Entities;
using AuctionApi.Core.Services;
using AuctionApi.Core.Tests.Fakes;
using Xunit;

namespace AuctionApi.Core.Tests.Services;

public class AttachmentServiceTests
{
    private readonly InMemoryUnitOfWork _uow = new();
    private readonly FakeFileStorage _storage = new();
    private readonly FakeCurrentUser _user = new() { UserId = 1, Role = "User" };

    private AttachmentService Create() => new(_uow, _storage, _user);

    private Auction SeedAuction(int ownerId = 1)
    {
        var a = new Auction
        {
            Title = "X", Description = "d", StartingPrice = 10,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(7),
            UserId = ownerId, IsActive = true,
        };
        _uow.Auctions.AddAsync(a);
        return a;
    }

    private static UploadedFile File(string name, string contentType, long size = 100) =>
        new(name, contentType, size, new MemoryStream(new byte[size]));

    [Fact]
    public async Task AddToAuction_as_owner_with_allowed_extension_saves()
    {
        var a = SeedAuction();

        var saved = (await Create().AddToAuctionAsync(a.Id, new[] { File("photo.jpg", "image/jpeg") })).ToList();

        Assert.Single(saved);
        Assert.Contains(saved[0].StoredFileName, _storage.Saved);
    }

    [Fact]
    public async Task AddToAuction_with_disallowed_extension_skips_file()
    {
        var a = SeedAuction();

        var saved = (await Create().AddToAuctionAsync(a.Id, new[] { File("evil.exe", "application/octet-stream") })).ToList();

        Assert.Empty(saved);
        Assert.Empty(_storage.Saved);
    }

    [Fact]
    public async Task AddToAuction_as_non_owner_throws_Forbidden()
    {
        var a = SeedAuction(ownerId: 99);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Create().AddToAuctionAsync(a.Id, new[] { File("photo.jpg", "image/jpeg") }));
    }

    [Fact]
    public async Task AddToAuction_with_oversized_file_throws_ValidationException()
    {
        var a = SeedAuction();
        // MaxFileSize in the fake is 1 MB
        var tooBig = new UploadedFile("huge.jpg", "image/jpeg", 2 * 1024 * 1024, new MemoryStream(new byte[2 * 1024 * 1024]));

        await Assert.ThrowsAsync<ValidationException>(() =>
            Create().AddToAuctionAsync(a.Id, new[] { tooBig }));
    }

    [Fact]
    public async Task AddToAuction_for_missing_auction_throws_NotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Create().AddToAuctionAsync(999, new[] { File("photo.jpg", "image/jpeg") }));
    }

    [Fact]
    public async Task Delete_as_owner_removes_attachment_and_file()
    {
        var a = SeedAuction();
        var saved = (await Create().AddToAuctionAsync(a.Id, new[] { File("photo.jpg", "image/jpeg") })).ToList();
        var att = saved[0];

        await Create().DeleteAsync(a.Id, att.Id);

        Assert.Contains(att.StoredFileName, _storage.Deleted);
        Assert.Empty(_uow.Attachments.All);
    }

    [Fact]
    public async Task Delete_as_non_owner_throws_Forbidden()
    {
        // Owner adds the file, then a different user tries to delete it.
        var a = SeedAuction(ownerId: 1);
        var saved = (await Create().AddToAuctionAsync(a.Id, new[] { File("photo.jpg", "image/jpeg") })).ToList();

        _user.UserId = 2;
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            Create().DeleteAsync(a.Id, saved[0].Id));
    }
}
