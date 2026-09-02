using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFacultyCandidateRankings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FacultyCandidateRankings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rank = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacultyCandidateRankings", x => x.Id);
                    table.CheckConstraint("CK_FacultyCandidateRankings_Rank", "[Rank] >= 1");
                    table.ForeignKey(
                        name: "FK_FacultyCandidateRankings_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FacultyCandidateRankings_Users_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacultyCandidateRankings_ProjectId_Rank",
                table: "FacultyCandidateRankings",
                columns: new[] { "ProjectId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_FacultyCandidateRankings_ProjectId_StudentUserId",
                table: "FacultyCandidateRankings",
                columns: new[] { "ProjectId", "StudentUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacultyCandidateRankings_StudentUserId",
                table: "FacultyCandidateRankings",
                column: "StudentUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacultyCandidateRankings");
        }
    }
}
