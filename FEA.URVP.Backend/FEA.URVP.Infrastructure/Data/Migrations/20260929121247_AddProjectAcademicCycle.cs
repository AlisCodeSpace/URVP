using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectAcademicCycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SemesterId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE p
                SET p.SemesterId = s.Id
                FROM Projects AS p
                CROSS APPLY (
                    SELECT TOP (1) Id
                    FROM Semesters
                    ORDER BY CASE WHEN IsActive = 1 THEN 0 ELSE 1 END, UpdatedAt DESC
                ) AS s
                WHERE p.SemesterId IS NULL;
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM Projects WHERE SemesterId IS NULL)
                    THROW 50000, 'Existing projects need an academic cycle, but no semester exists yet.', 1;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "SemesterId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectAlumni",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SemesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SemesterName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    StudentUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StudentEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    StudentRank = table.Column<byte>(type: "tinyint", nullable: true),
                    FacultyRank = table.Column<byte>(type: "tinyint", nullable: true),
                    WasConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAlumni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectAlumni_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectAlumni_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SemesterId",
                table: "Projects",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SemesterId_Status",
                table: "Projects",
                columns: new[] { "SemesterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAlumni_ProjectId_ArchivedAt",
                table: "ProjectAlumni",
                columns: new[] { "ProjectId", "ArchivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAlumni_ProjectId_SemesterId_StudentUserId",
                table: "ProjectAlumni",
                columns: new[] { "ProjectId", "SemesterId", "StudentUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAlumni_SemesterId",
                table: "ProjectAlumni",
                column: "SemesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Semesters_SemesterId",
                table: "Projects",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Semesters_SemesterId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "ProjectAlumni");

            migrationBuilder.DropIndex(
                name: "IX_Projects_SemesterId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_SemesterId_Status",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SemesterId",
                table: "Projects");
        }
    }
}
