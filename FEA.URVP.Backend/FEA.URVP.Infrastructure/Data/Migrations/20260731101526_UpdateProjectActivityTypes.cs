using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectActivityTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivityType",
                table: "Projects");

            migrationBuilder.AddColumn<string>(
                name: "ActivityTypes",
                table: "Projects",
                type: "nvarchar(2000)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivityTypes",
                table: "Projects");

            migrationBuilder.AddColumn<byte>(
                name: "ActivityType",
                table: "Projects",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
