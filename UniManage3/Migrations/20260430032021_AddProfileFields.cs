using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniManage3.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "Students",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "Students",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Students",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Students",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Students",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Students",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NICNumber",
                table: "Students",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Students",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Students",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "Lecturers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "Lecturers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Lecturers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Lecturers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Lecturers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Lecturers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NICNumber",
                table: "Lecturers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Lecturers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Lecturers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "Administrators",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "Administrators",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Administrators",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Administrators",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Administrators",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Administrators",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NICNumber",
                table: "Administrators",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Administrators",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Administrators",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "NICNumber",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "NICNumber",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "Administrators");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "Administrators");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Administrators");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Administrators");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Administrators");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Administrators");

            migrationBuilder.DropColumn(
                name: "NICNumber",
                table: "Administrators");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Administrators");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Administrators");
        }
    }
}
