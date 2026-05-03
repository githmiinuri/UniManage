using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniManage3.Migrations
{
    using UniManage3.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260501020000_DropDuplicateProfileColumns")]
    public partial class DropDuplicateProfileColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop duplicated credential/role columns from profile tables
            // Administrators
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Administrators");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Administrators");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Administrators");

            // Lecturers
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Lecturers");

            // Students
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Students");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-create dropped columns as nullable for rollback
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Administrators",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Administrators",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Administrators",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Lecturers",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Lecturers",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Lecturers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Students",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Students",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Students",
                type: "int",
                nullable: true);
        }
    }
}
