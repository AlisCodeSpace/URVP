using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManualPlacementAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Source",
                table: "Placements",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_MatchingRuns_ManualPerSemester",
                table: "MatchingRuns",
                column: "SemesterId",
                unique: true,
                filter: "[AlgorithmVersion] = N'manual/v1'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchingRuns_ManualPerSemester",
                table: "MatchingRuns");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Placements");
        }
    }
}
