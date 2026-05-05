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
            // Drop Email column if it exists (MySQL may not support DROP COLUMN IF EXISTS) - use prepared statement
            migrationBuilder.Sql(@"
SET @exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='Lecturers' AND COLUMN_NAME='Email');
SET @s = IF(@exists>0, 'ALTER TABLE `Lecturers` DROP COLUMN `Email`', 'SELECT 0');
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            // Drop PasswordHash column if it exists
            migrationBuilder.Sql(@"
SET @exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='Lecturers' AND COLUMN_NAME='PasswordHash');
SET @s = IF(@exists>0, 'ALTER TABLE `Lecturers` DROP COLUMN `PasswordHash`', 'SELECT 0');
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            // If `RoleId` exists and `UserId` does not, rename it to `UserId`.
            // Otherwise, if `UserId` does not exist, add it as an INT column.
            migrationBuilder.Sql(@"
SET @existsRole = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='Lecturers' AND COLUMN_NAME='RoleId');
SET @existsUser = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='Lecturers' AND COLUMN_NAME='UserId');
SET @s = IF(@existsRole>0 AND @existsUser=0, 'ALTER TABLE `Lecturers` CHANGE `RoleId` `UserId` INT', IF(@existsUser=0, 'ALTER TABLE `Lecturers` ADD COLUMN `UserId` INT', 'SELECT 0'));
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            // Create index only if not exists (use prepared statement for compatibility)
            migrationBuilder.Sql(@"
SET @idx = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='Lecturers' AND INDEX_NAME='IX_Lecturers_UserId');
SET @s = IF(@idx=0, 'CREATE INDEX `IX_Lecturers_UserId` ON `Lecturers` (`UserId`)', 'SELECT 0');
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");

            // Add foreign key only if it does not already exist
            migrationBuilder.Sql(@"
SET @fk = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE() AND CONSTRAINT_NAME='FK_Lecturers_Users_UserId');
SET @s = IF(@fk=0, 'ALTER TABLE `Lecturers` ADD CONSTRAINT `FK_Lecturers_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE', 'SELECT 0');
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
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
