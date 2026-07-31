using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNameAndAffiliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Affiliation",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "American University of Beirut");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE [Users]
                SET [UserName] = CASE
                    WHEN CHARINDEX('@', [Email]) > 1 THEN LEFT([Email], CHARINDEX('@', [Email]) - 1)
                    ELSE [Email]
                END
                WHERE [UserName] = '' OR [UserName] IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Affiliation",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Users");
        }
    }
}
