using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FEA.URVP.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeIrbOptionalAndReplaceCitiWithCv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FileStorage_FileSize",
                table: "FileStorage");

            migrationBuilder.RenameColumn(
                name: "CitiFileId",
                table: "StudentProfiles",
                newName: "CvFileId");

            migrationBuilder.AlterColumn<byte>(
                name: "IrbStage",
                table: "Projects",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.Sql(
                "UPDATE [FileStorage] SET [FileCategory] = 'Cv' WHERE [FileCategory] = 'CitiCertification'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FileStorage_FileSize",
                table: "FileStorage",
                sql: "(([FileCategory] IN ('Transcript', 'Cv') AND [FileSize] <= 10485760) OR ([FileCategory] = 'Poster' AND [FileSize] <= 5242880))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FileStorage_FileSize",
                table: "FileStorage");

            migrationBuilder.Sql(
                "UPDATE [FileStorage] SET [FileCategory] = 'CitiCertification' WHERE [FileCategory] = 'Cv'");

            migrationBuilder.RenameColumn(
                name: "CvFileId",
                table: "StudentProfiles",
                newName: "CitiFileId");

            migrationBuilder.AlterColumn<byte>(
                name: "IrbStage",
                table: "Projects",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_FileStorage_FileSize",
                table: "FileStorage",
                sql: "(([FileCategory] IN ('Transcript', 'CitiCertification') AND [FileSize] <= 10485760) OR ([FileCategory] = 'Poster' AND [FileSize] <= 5242880))");
        }
    }
}
