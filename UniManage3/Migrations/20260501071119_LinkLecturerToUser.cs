using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniManage3.Migrations
{
    /// <inheritdoc />
    public partial class LinkLecturerToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Lecturers");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Lecturers",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Lecturers_UserId",
                table: "Lecturers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lecturers_Users_UserId",
                table: "Lecturers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lecturers_Users_UserId",
                table: "Lecturers");

            migrationBuilder.DropIndex(
                name: "IX_Lecturers_UserId",
                table: "Lecturers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Lecturers",
                newName: "RoleId");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Lecturers",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Lecturers",
                type: "varchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
