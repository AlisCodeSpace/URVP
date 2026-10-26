using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectResearchAssistants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectResearchAssistants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AddedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectResearchAssistants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectResearchAssistants_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectResearchAssistants_Users_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectResearchAssistants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResearchAssistants_AddedByUserId",
                table: "ProjectResearchAssistants",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResearchAssistants_ProjectId",
                table: "ProjectResearchAssistants",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResearchAssistants_ProjectId_Email",
                table: "ProjectResearchAssistants",
                columns: new[] { "ProjectId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResearchAssistants_ProjectId_UserId",
                table: "ProjectResearchAssistants",
                columns: new[] { "ProjectId", "UserId" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResearchAssistants_UserId",
                table: "ProjectResearchAssistants",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectResearchAssistants");
        }
    }
}
