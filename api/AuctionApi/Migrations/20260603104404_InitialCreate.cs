using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuctionApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Auctions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    StartingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Auctions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuctionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_Auctions_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BidDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AuctionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bids_Auctions_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bids_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "IsActive", "Name", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, "admin@auctionlabb.com", true, "Admin", "$2a$11$2TTWZl03PadLy6RCW70WlOmuz8F1FqPupNCOePA7gdOF/gl97ZRwu", "Admin" },
                    { 2, "curator@auctionlabb.com", true, "Gallery Curator", "$2a$11$2TTWZl03PadLy6RCW70WlOmuz8F1FqPupNCOePA7gdOF/gl97ZRwu", "User" },
                    { 3, "collector@auctionlabb.com", true, "Art Collector", "$2a$11$2TTWZl03PadLy6RCW70WlOmuz8F1FqPupNCOePA7gdOF/gl97ZRwu", "User" }
                });

            migrationBuilder.InsertData(
                table: "Auctions",
                columns: new[] { "Id", "Description", "EndDate", "IsActive", "StartDate", "StartingPrice", "Title", "UserId" },
                values: new object[,]
                {
                    { 1, "A delicate floral still life watercolor on paper by Pieter van Loo (Haarlem 1735–1784). Van Loo was a respected member of the Haarlem Painters' Guild, officially registered as a 'painter of flowers.' This classical Dutch botanical painting depicts a richly arranged bouquet in a vase, showcasing van Loo's meticulous attention to botanical detail and his mastery of the watercolor medium. Comparable works by van Loo have sold at European auctions in the range of €5,000–€15,000.", new DateTime(2026, 7, 15, 20, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 5, 20, 12, 0, 0, 0, DateTimeKind.Utc), 8500.00m, "Vase with Flowers — Pieter van Loo (c. 1745–1784)", 2 },
                    { 2, "A dramatic allegorical masterpiece of the Dutch Golden Age by Jan Asselijn (c. 1610–1652). Painted in oil on canvas around 1650, this life-sized depiction of a swan fiercely defending its nest has become one of the most iconic paintings in the Rijksmuseum collection. The work has been interpreted as a political allegory of the Dutch Republic defending itself against its enemies. As a national treasure of the Netherlands held in the Rijksmuseum, this painting is considered virtually priceless. Auction estimates for comparable Dutch Golden Age masterworks by major artists range from $2,000,000 to $5,000,000+.", new DateTime(2026, 8, 1, 20, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 5, 25, 10, 0, 0, 0, DateTimeKind.Utc), 2200000.00m, "The Threatened Swan — Jan Asselijn (c. 1650)", 2 },
                    { 3, "A tranquil landscape masterpiece by Paul Joseph Constantin Gabriël (1828–1903), a leading painter of the Hague School, often referred to as Dutch Impressionism. This oil on canvas (102 x 66 cm) from c. 1889 is one of the most celebrated Dutch landscape paintings of the 19th century and is part of the Rijksmuseum's permanent collection. The painting captures a quintessential Dutch polder scene with a windmill under a luminous sky. Gabriël's works have achieved auction prices ranging from €15,000 for smaller studies to €132,000 for major canvases, with this being among his finest.", new DateTime(2026, 8, 10, 20, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2026, 6, 1, 14, 0, 0, 0, DateTimeKind.Utc), 45000.00m, "A Windmill on a Polder Waterway, 'In the Month of July' — Paul Joseph Constantin Gabriël (c. 1889)", 2 }
                });

            migrationBuilder.InsertData(
                table: "Attachments",
                columns: new[] { "Id", "AuctionId", "ContentType", "FileName", "FileSize", "StoredFileName", "UploadedAt" },
                values: new object[,]
                {
                    { 1, 1, "image/jpeg", "Vase with Flowers.jpg", 3014991L, "vase-with-flowers.jpg", new DateTime(2026, 5, 20, 12, 5, 0, 0, DateTimeKind.Utc) },
                    { 2, 2, "image/jpeg", "The Threatened Swan.jpg", 4674966L, "the-threatened-swan.jpg", new DateTime(2026, 5, 25, 10, 5, 0, 0, DateTimeKind.Utc) },
                    { 3, 3, "image/jpeg", "In the Month of July.jpg", 6042351L, "in-the-month-of-july.jpg", new DateTime(2026, 6, 1, 14, 5, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Bids",
                columns: new[] { "Id", "Amount", "AuctionId", "BidDate", "UserId" },
                values: new object[,]
                {
                    { 1, 10000.00m, 1, new DateTime(2026, 5, 22, 14, 30, 0, 0, DateTimeKind.Utc), 3 },
                    { 2, 12500.00m, 1, new DateTime(2026, 5, 25, 9, 15, 0, 0, DateTimeKind.Utc), 3 },
                    { 3, 2500000.00m, 2, new DateTime(2026, 5, 28, 11, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 4, 50000.00m, 3, new DateTime(2026, 6, 3, 16, 45, 0, 0, DateTimeKind.Utc), 3 },
                    { 5, 62500.00m, 3, new DateTime(2026, 6, 5, 10, 20, 0, 0, DateTimeKind.Utc), 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_AuctionId",
                table: "Attachments",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_UserId",
                table: "Auctions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_AuctionId",
                table: "Bids",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_UserId",
                table: "Bids",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "Bids");

            migrationBuilder.DropTable(
                name: "Auctions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
