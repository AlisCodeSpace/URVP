using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsArticleImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageFileIds",
                table: "NewsArticles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FileStorage_EntityType",
                table: "FileStorage");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FileStorage_FileSize",
                table: "FileStorage");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FileStorage_EntityType",
                table: "FileStorage",
                sql: "[EntityType] IN ('StudentProfile', 'Workshop', 'NewsArticle')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FileStorage_FileSize",
                table: "FileStorage",
                sql: "(([FileCategory] IN ('Transcript', 'Cv') AND [FileSize] <= 10485760) OR ([FileCategory] IN ('Poster', 'NewsImage') AND [FileSize] <= 5242880))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileIds",
                table: "NewsArticles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FileStorage_EntityType",
                table: "FileStorage");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FileStorage_FileSize",
                table: "FileStorage");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FileStorage_EntityType",
                table: "FileStorage",
                sql: "[EntityType] IN ('StudentProfile', 'Workshop')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FileStorage_FileSize",
                table: "FileStorage",
                sql: "(([FileCategory] IN ('Transcript', 'Cv') AND [FileSize] <= 10485760) OR ([FileCategory] = 'Poster' AND [FileSize] <= 5242880))");
        }
    }
}
