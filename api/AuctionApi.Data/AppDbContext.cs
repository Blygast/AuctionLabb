using AuctionApi.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Auction> Auctions => Set<Auction>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<Auction>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.StartingPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Auctions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Bids)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Auction)
                .WithMany(a => a.Bids)
                .HasForeignKey(e => e.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(500);

            entity.HasOne(e => e.Auction)
                .WithMany(a => a.Attachments)
                .HasForeignKey(e => e.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Admin",
                Email = "admin@auctionlabb.com",
                PasswordHash = "$2a$11$2TTWZl03PadLy6RCW70WlOmuz8F1FqPupNCOePA7gdOF/gl97ZRwu",
                Role = "Admin",
                IsActive = true
            },
            new User
            {
                Id = 2,
                Name = "Gallery Curator",
                Email = "curator@auctionlabb.com",
                PasswordHash = "$2a$11$2TTWZl03PadLy6RCW70WlOmuz8F1FqPupNCOePA7gdOF/gl97ZRwu",
                Role = "User",
                IsActive = true
            },
            new User
            {
                Id = 3,
                Name = "Art Collector",
                Email = "collector@auctionlabb.com",
                PasswordHash = "$2a$11$2TTWZl03PadLy6RCW70WlOmuz8F1FqPupNCOePA7gdOF/gl97ZRwu",
                Role = "User",
                IsActive = true
            }
        );

        modelBuilder.Entity<Auction>().HasData(
            new Auction
            {
                Id = 1,
                Title = "Vase with Flowers — Pieter van Loo (c. 1745–1784)",
                Description = "A delicate floral still life watercolor on paper by Pieter van Loo (Haarlem 1735–1784). Van Loo was a respected member of the Haarlem Painters' Guild, officially registered as a 'painter of flowers.' This classical Dutch botanical painting depicts a richly arranged bouquet in a vase, showcasing van Loo's meticulous attention to botanical detail and his mastery of the watercolor medium. Comparable works by van Loo have sold at European auctions in the range of €5,000–€15,000.",
                StartingPrice = 8500.00m,
                StartDate = new DateTime(2026, 5, 20, 12, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 7, 15, 20, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                UserId = 2
            },
            new Auction
            {
                Id = 2,
                Title = "The Threatened Swan — Jan Asselijn (c. 1650)",
                Description = "A dramatic allegorical masterpiece of the Dutch Golden Age by Jan Asselijn (c. 1610–1652). Painted in oil on canvas around 1650, this life-sized depiction of a swan fiercely defending its nest has become one of the most iconic paintings in the Rijksmuseum collection. The work has been interpreted as a political allegory of the Dutch Republic defending itself against its enemies. As a national treasure of the Netherlands held in the Rijksmuseum, this painting is considered virtually priceless. Auction estimates for comparable Dutch Golden Age masterworks by major artists range from $2,000,000 to $5,000,000+.",
                StartingPrice = 2200000.00m,
                StartDate = new DateTime(2026, 5, 25, 10, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 8, 1, 20, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                UserId = 2
            },
            new Auction
            {
                Id = 3,
                Title = "A Windmill on a Polder Waterway, 'In the Month of July' — Paul Joseph Constantin Gabriël (c. 1889)",
                Description = "A tranquil landscape masterpiece by Paul Joseph Constantin Gabriël (1828–1903), a leading painter of the Hague School, often referred to as Dutch Impressionism. This oil on canvas (102 x 66 cm) from c. 1889 is one of the most celebrated Dutch landscape paintings of the 19th century and is part of the Rijksmuseum's permanent collection. The painting captures a quintessential Dutch polder scene with a windmill under a luminous sky. Gabriël's works have achieved auction prices ranging from €15,000 for smaller studies to €132,000 for major canvases, with this being among his finest.",
                StartingPrice = 45000.00m,
                StartDate = new DateTime(2026, 6, 1, 14, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 8, 10, 20, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                UserId = 2
            }
        );

        modelBuilder.Entity<Bid>().HasData(
            new Bid { Id = 1, Amount = 10000.00m, BidDate = new DateTime(2026, 5, 22, 14, 30, 0, DateTimeKind.Utc), UserId = 3, AuctionId = 1 },
            new Bid { Id = 2, Amount = 12500.00m, BidDate = new DateTime(2026, 5, 25, 9, 15, 0, DateTimeKind.Utc), UserId = 3, AuctionId = 1 },
            new Bid { Id = 3, Amount = 2500000.00m, BidDate = new DateTime(2026, 5, 28, 11, 0, 0, DateTimeKind.Utc), UserId = 3, AuctionId = 2 },
            new Bid { Id = 4, Amount = 50000.00m, BidDate = new DateTime(2026, 6, 3, 16, 45, 0, DateTimeKind.Utc), UserId = 3, AuctionId = 3 },
            new Bid { Id = 5, Amount = 62500.00m, BidDate = new DateTime(2026, 6, 5, 10, 20, 0, DateTimeKind.Utc), UserId = 3, AuctionId = 3 }
        );

        modelBuilder.Entity<Attachment>().HasData(
            new Attachment
            {
                Id = 1,
                FileName = "Vase with Flowers.jpg",
                ContentType = "image/jpeg",
                StoredFileName = "vase-with-flowers.jpg",
                FileSize = 3014991,
                UploadedAt = new DateTime(2026, 5, 20, 12, 5, 0, DateTimeKind.Utc),
                AuctionId = 1
            },
            new Attachment
            {
                Id = 2,
                FileName = "The Threatened Swan.jpg",
                ContentType = "image/jpeg",
                StoredFileName = "the-threatened-swan.jpg",
                FileSize = 4674966,
                UploadedAt = new DateTime(2026, 5, 25, 10, 5, 0, DateTimeKind.Utc),
                AuctionId = 2
            },
            new Attachment
            {
                Id = 3,
                FileName = "In the Month of July.jpg",
                ContentType = "image/jpeg",
                StoredFileName = "in-the-month-of-july.jpg",
                FileSize = 6042351,
                UploadedAt = new DateTime(2026, 6, 1, 14, 5, 0, DateTimeKind.Utc),
                AuctionId = 3
            }
        );
    }
}
