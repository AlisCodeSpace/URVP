using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ResearchArea = table.Column<byte>(type: "tinyint", nullable: false),
                    IrbStage = table.Column<byte>(type: "tinyint", nullable: false),
                    BriefDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ActivityType = table.Column<byte>(type: "tinyint", nullable: false),
                    VolunteersRequired = table.Column<int>(type: "int", nullable: false),
                    VolunteersFilled = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MinQualifications = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AdditionalComments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    FacultyNameSnapshot = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AffiliationSnapshot = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EmailSnapshot = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.CheckConstraint("CK_Projects_VolunteersFilled", "[VolunteersFilled] >= 0");
                    table.CheckConstraint("CK_Projects_VolunteersRequired", "[VolunteersRequired] >= 1");
                    table.ForeignKey(
                        name: "FK_Projects_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedByUserId",
                table: "Projects",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status_CreatedAt",
                table: "Projects",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
