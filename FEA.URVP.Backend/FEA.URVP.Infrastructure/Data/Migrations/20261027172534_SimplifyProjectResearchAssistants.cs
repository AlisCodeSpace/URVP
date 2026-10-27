using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyProjectResearchAssistants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Emails are matched against the lowercase address stored at sign-in.
            migrationBuilder.Sql("UPDATE [ProjectResearchAssistants] SET [Email] = LOWER(LTRIM(RTRIM([Email])))");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectResearchAssistants_Users_UserId",
                table: "ProjectResearchAssistants");

            migrationBuilder.DropIndex(
                name: "IX_ProjectResearchAssistants_ProjectId",
                table: "ProjectResearchAssistants");

            migrationBuilder.DropIndex(
                name: "IX_ProjectResearchAssistants_ProjectId_UserId",
                table: "ProjectResearchAssistants");

            migrationBuilder.DropIndex(
                name: "IX_ProjectResearchAssistants_UserId",
                table: "ProjectResearchAssistants");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ProjectResearchAssistants");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ProjectResearchAssistants");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResearchAssistants_Email",
                table: "ProjectResearchAssistants",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectResearchAssistants_Email",
                table: "ProjectResearchAssistants");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ProjectResearchAssistants",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ProjectResearchAssistants",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResearchAssistants_ProjectId",
                table: "ProjectResearchAssistants",
                column: "ProjectId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectResearchAssistants_Users_UserId",
                table: "ProjectResearchAssistants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
