using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniManage3.Migrations
{
    using UniManage3.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260501010101_LinkProfilesToUsers")]
    public partial class LinkProfilesToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add UserId columns / indexes / foreign keys only if they don't already exist
            // Use dynamic SQL with prepared statements so this migration is idempotent across MySQL versions

            // Administrators
            migrationBuilder.Sql(@"
SET @c = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Administrators' AND COLUMN_NAME = 'UserId');
SET @s = IF(@c = 0, 'ALTER TABLE `Administrators` ADD `UserId` int NULL', 'SELECT 0');
PREPARE stmt FROM @s; EXECUTE stmt; DEALLOCATE PREPARE stmt;
");

            migrationBuilder.Sql(@"
SET @idx = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Administrators' AND INDEX_NAME = 'IX_Administrators_UserId');
SET @sidx = IF(@idx = 0, 'CREATE INDEX `IX_Administrators_UserId` ON `Administrators` (`UserId`)', 'SELECT 0');
PREPARE stmt2 FROM @sidx; EXECUTE stmt2; DEALLOCATE PREPARE stmt2;
");

            migrationBuilder.Sql(@"
SET @fk = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Administrators' AND CONSTRAINT_NAME = 'FK_Administrators_Users_UserId');
SET @sfk = IF(@fk = 0, 'ALTER TABLE `Administrators` ADD CONSTRAINT `FK_Administrators_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE', 'SELECT 0');
PREPARE stmt3 FROM @sfk; EXECUTE stmt3; DEALLOCATE PREPARE stmt3;
");

            // Lecturers
            migrationBuilder.Sql(@"
SET @c = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Lecturers' AND COLUMN_NAME = 'UserId');
SET @s = IF(@c = 0, 'ALTER TABLE `Lecturers` ADD `UserId` int NULL', 'SELECT 0');
PREPARE stmt4 FROM @s; EXECUTE stmt4; DEALLOCATE PREPARE stmt4;
");

            migrationBuilder.Sql(@"
SET @idx = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Lecturers' AND INDEX_NAME = 'IX_Lecturers_UserId');
SET @sidx = IF(@idx = 0, 'CREATE INDEX `IX_Lecturers_UserId` ON `Lecturers` (`UserId`)', 'SELECT 0');
PREPARE stmt5 FROM @sidx; EXECUTE stmt5; DEALLOCATE PREPARE stmt5;
");

            migrationBuilder.Sql(@"
SET @fk = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Lecturers' AND CONSTRAINT_NAME = 'FK_Lecturers_Users_UserId');
SET @sfk = IF(@fk = 0, 'ALTER TABLE `Lecturers` ADD CONSTRAINT `FK_Lecturers_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE', 'SELECT 0');
PREPARE stmt6 FROM @sfk; EXECUTE stmt6; DEALLOCATE PREPARE stmt6;
");

            // Students
            migrationBuilder.Sql(@"
SET @c = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Students' AND COLUMN_NAME = 'UserId');
SET @s = IF(@c = 0, 'ALTER TABLE `Students` ADD `UserId` int NULL', 'SELECT 0');
PREPARE stmt7 FROM @s; EXECUTE stmt7; DEALLOCATE PREPARE stmt7;
");

            migrationBuilder.Sql(@"
SET @idx = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Students' AND INDEX_NAME = 'IX_Students_UserId');
SET @sidx = IF(@idx = 0, 'CREATE INDEX `IX_Students_UserId` ON `Students` (`UserId`)', 'SELECT 0');
PREPARE stmt8 FROM @sidx; EXECUTE stmt8; DEALLOCATE PREPARE stmt8;
");

            migrationBuilder.Sql(@"
SET @fk = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Students' AND CONSTRAINT_NAME = 'FK_Students_Users_UserId');
SET @sfk = IF(@fk = 0, 'ALTER TABLE `Students` ADD CONSTRAINT `FK_Students_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE', 'SELECT 0');
PREPARE stmt9 FROM @sfk; EXECUTE stmt9; DEALLOCATE PREPARE stmt9;
");

            // Note: we do not drop existing duplicate columns here to avoid accidental data loss.
            // If you want to remove `Email`, `PasswordHash`, `RoleId` from profile tables,
            // perform a controlled data migration and then drop columns in a separate migration.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: re-create the dropped columns as nullable and remove foreign keys
            migrationBuilder.Sql(@"ALTER TABLE `Administrators` ADD COLUMN IF NOT EXISTS `Email` varchar(256) NULL;");
            migrationBuilder.Sql(@"ALTER TABLE `Administrators` ADD COLUMN IF NOT EXISTS `PasswordHash` varchar(512) NULL;");
            migrationBuilder.Sql(@"ALTER TABLE `Administrators` ADD COLUMN IF NOT EXISTS `RoleId` int NULL;");
            migrationBuilder.Sql(@"ALTER TABLE `Administrators` DROP FOREIGN KEY IF EXISTS `FK_Administrators_Users_UserId`;");
            migrationBuilder.Sql(@"ALTER TABLE `Administrators` DROP INDEX IF EXISTS `IX_Administrators_UserId`;");
            migrationBuilder.Sql(@"ALTER TABLE `Administrators` DROP COLUMN IF EXISTS `UserId`;");

            migrationBuilder.Sql(@"ALTER TABLE `Lecturers` ADD COLUMN IF NOT EXISTS `Email` varchar(256) NULL;");
            migrationBuilder.Sql(@"ALTER TABLE `Lecturers` ADD COLUMN IF NOT EXISTS `PasswordHash` varchar(512) NULL;");
            migrationBuilder.Sql(@"ALTER TABLE `Lecturers` ADD COLUMN IF NOT EXISTS `RoleId` int NULL;");
            migrationBuilder.Sql(@"ALTER TABLE `Lecturers` DROP FOREIGN KEY IF EXISTS `FK_Lecturers_Users_UserId`;");
            migrationBuilder.Sql(@"ALTER TABLE `Lecturers` DROP INDEX IF EXISTS `IX_Lecturers_UserId`;");
            migrationBuilder.Sql(@"ALTER TABLE `Lecturers` DROP COLUMN IF EXISTS `UserId`;");

            migrationBuilder.Sql(@"ALTER TABLE `Students` ADD COLUMN IF NOT EXISTS `Email` varchar(256) NULL;");
            migrationBuilder.Sql(@"ALTER TABLE `Students` ADD COLUMN IF NOT EXISTS `PasswordHash` varchar(512) NULL;");
            migrationBuilder.Sql(@"ALTER TABLE `Students` ADD COLUMN IF NOT EXISTS `RoleId` int NULL;");
            migrationBuilder.Sql(@"ALTER TABLE `Students` DROP FOREIGN KEY IF EXISTS `FK_Students_Users_UserId`;");
            migrationBuilder.Sql(@"ALTER TABLE `Students` DROP INDEX IF EXISTS `IX_Students_UserId`;");
            migrationBuilder.Sql(@"ALTER TABLE `Students` DROP COLUMN IF EXISTS `UserId`;");
        }
    }
}
