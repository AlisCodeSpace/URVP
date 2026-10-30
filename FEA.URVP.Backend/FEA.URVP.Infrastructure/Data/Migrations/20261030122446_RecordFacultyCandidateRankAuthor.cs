using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RecordFacultyCandidateRankAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RankedByUserId",
                table: "FacultyCandidateRankings",
                type: "uniqueidentifier",
                nullable: true);

            // Existing ranks predate this column. Credit the faculty member who posted the project.
            migrationBuilder.Sql(
                """
                UPDATE ranking
                SET ranking.RankedByUserId = project.CreatedByUserId
                FROM FacultyCandidateRankings AS ranking
                INNER JOIN Projects AS project ON project.Id = ranking.ProjectId
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "RankedByUserId",
                table: "FacultyCandidateRankings",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacultyCandidateRankings_RankedByUserId",
                table: "FacultyCandidateRankings",
                column: "RankedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FacultyCandidateRankings_Users_RankedByUserId",
                table: "FacultyCandidateRankings",
                column: "RankedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacultyCandidateRankings_Users_RankedByUserId",
                table: "FacultyCandidateRankings");

            migrationBuilder.DropIndex(
                name: "IX_FacultyCandidateRankings_RankedByUserId",
                table: "FacultyCandidateRankings");

            migrationBuilder.DropColumn(
                name: "RankedByUserId",
                table: "FacultyCandidateRankings");
        }
    }
}
