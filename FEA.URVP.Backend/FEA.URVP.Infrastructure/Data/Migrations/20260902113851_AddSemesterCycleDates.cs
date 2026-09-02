using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSemesterCycleDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CycleEnd",
                table: "Semesters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CycleStart",
                table: "Semesters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [Semesters]
                SET [CycleStart] = COALESCE([ApplicationWindowStart], [CreatedAt])
                WHERE [IsActive] = 1 AND [CycleStart] IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CycleEnd",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "CycleStart",
                table: "Semesters");
        }
    }
}
