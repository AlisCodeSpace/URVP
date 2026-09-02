using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchingRunsAndPlacements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchingRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SemesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    AlgorithmVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Seed = table.Column<int>(type: "int", nullable: false),
                    StudentsConsidered = table.Column<int>(type: "int", nullable: false),
                    ProjectsConsidered = table.Column<int>(type: "int", nullable: false),
                    SeatsAvailable = table.Column<int>(type: "int", nullable: false),
                    StudentsMatched = table.Column<int>(type: "int", nullable: false),
                    TieBreaksUsed = table.Column<int>(type: "int", nullable: false),
                    Warnings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchingRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchingRuns_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Placements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchingRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentRank = table.Column<byte>(type: "tinyint", nullable: false),
                    FacultyRank = table.Column<byte>(type: "tinyint", nullable: false),
                    ResolvedByTieBreak = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Placements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Placements_MatchingRuns_MatchingRunId",
                        column: x => x.MatchingRunId,
                        principalTable: "MatchingRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Placements_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Placements_Users_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchingRuns_SemesterId_Status",
                table: "MatchingRuns",
                columns: new[] { "SemesterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Placements_MatchingRunId_StudentUserId",
                table: "Placements",
                columns: new[] { "MatchingRunId", "StudentUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Placements_ProjectId_Status",
                table: "Placements",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Placements_StudentUserId_Status",
                table: "Placements",
                columns: new[] { "StudentUserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Placements");

            migrationBuilder.DropTable(
                name: "MatchingRuns");
        }
    }
}
