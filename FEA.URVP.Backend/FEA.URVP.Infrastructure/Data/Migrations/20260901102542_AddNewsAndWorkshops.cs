using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsAndWorkshops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FileStorage_EntityType",
                table: "FileStorage");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FileStorage_FileSize",
                table: "FileStorage");

            migrationBuilder.CreateTable(
                name: "NewsArticles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Excerpt = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Ticker = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Featured = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workshops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Date = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Time = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RegistrationUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PosterFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PosterAlt = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workshops", x => x.Id);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_FileStorage_EntityType",
                table: "FileStorage",
                sql: "[EntityType] IN ('StudentProfile', 'Workshop')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FileStorage_FileSize",
                table: "FileStorage",
                sql: "(([FileCategory] IN ('Transcript', 'CitiCertification') AND [FileSize] <= 10485760) OR ([FileCategory] = 'Poster' AND [FileSize] <= 5242880))");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_PublishedAt",
                table: "NewsArticles",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_Slug",
                table: "NewsArticles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_SortOrder",
                table: "Workshops",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsArticles");

            migrationBuilder.DropTable(
                name: "Workshops");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FileStorage_EntityType",
                table: "FileStorage");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FileStorage_FileSize",
                table: "FileStorage");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FileStorage_EntityType",
                table: "FileStorage",
                sql: "[EntityType] IN ('StudentProfile')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FileStorage_FileSize",
                table: "FileStorage",
                sql: "([FileCategory] IN ('Transcript', 'CitiCertification') AND [FileSize] <= 10485760)");
        }
    }
}
