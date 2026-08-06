using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFileStorageAndProfileFileIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CitiFileName",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "TranscriptFileName",
                table: "StudentProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "CitiFileId",
                table: "StudentProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TranscriptFileId",
                table: "StudentProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FileStorage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentHash = table.Column<byte[]>(type: "varbinary(32)", maxLength: 32, nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileStorage", x => x.Id);
                    table.CheckConstraint("CK_FileStorage_EntityType", "[EntityType] IN ('StudentProfile')");
                    table.CheckConstraint("CK_FileStorage_FileSize", "([FileCategory] IN ('Transcript', 'CitiCertification') AND [FileSize] <= 10485760)");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileStorage_ContentHash",
                table: "FileStorage",
                column: "ContentHash",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FileStorage_Entity",
                table: "FileStorage",
                columns: new[] { "EntityType", "EntityId", "FileCategory", "IsDeleted" },
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileStorage");

            migrationBuilder.DropColumn(
                name: "CitiFileId",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "TranscriptFileId",
                table: "StudentProfiles");

            migrationBuilder.AddColumn<string>(
                name: "CitiFileName",
                table: "StudentProfiles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranscriptFileName",
                table: "StudentProfiles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
