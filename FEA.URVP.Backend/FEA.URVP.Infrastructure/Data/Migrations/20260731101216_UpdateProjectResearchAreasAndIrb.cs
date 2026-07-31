using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectResearchAreasAndIrb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResearchArea",
                table: "Projects");

            migrationBuilder.AddColumn<string>(
                name: "ResearchAreas",
                table: "Projects",
                type: "nvarchar(2000)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResearchAreas",
                table: "Projects");

            migrationBuilder.AddColumn<byte>(
                name: "ResearchArea",
                table: "Projects",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
