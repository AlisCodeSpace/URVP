using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Degree = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ExpectedGraduationYear = table.Column<int>(type: "int", nullable: false),
                    Languages = table.Column<string>(type: "nvarchar(2000)", nullable: false),
                    OtherLanguages = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CompletedCredits = table.Column<bool>(type: "bit", nullable: false),
                    CumulativeAverage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ResearchTopics = table.Column<string>(type: "nvarchar(2000)", nullable: false),
                    Publications = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TranscriptFileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CitiFileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Availability = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_UserId",
                table: "StudentProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentProfiles");
        }
    }
}
