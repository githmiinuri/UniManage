using System;
using System.IO;
using System.Linq;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace UniManage3.DbMigrator
{
    public class MigrationHelper
    {
        // Change from top-level statements to a standard Method
        public static int RunManualMigrations()
        {
            string FindFileUpwards(string fileName)
            {
                var starts = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
                foreach (var start in starts)
                {
                    DirectoryInfo dir = new DirectoryInfo(start);
                    for (int i = 0; i < 8 && dir != null; i++)
                    {
                        var candidate = Path.Combine(dir.FullName, fileName);
                        if (File.Exists(candidate)) return candidate;
                        dir = dir.Parent;
                    }
                }
                return null;
            }

            string settingsPath = FindFileUpwards("appsettings.json");
            if (settingsPath == null)
            {
                Console.Error.WriteLine("Could not find appsettings.json. Make sure it exists at the project or solution root.");
                return 1;
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(settingsPath) ?? Directory.GetCurrentDirectory())
                .AddJsonFile(Path.GetFileName(settingsPath), optional: false, reloadOnChange: true);

            var config = builder.Build();
            var cs = config.GetConnectionString("DefaultConnection");

            var csBuilder = new MySqlConnector.MySqlConnectionStringBuilder(cs)
            {
                AllowUserVariables = true
            };

            var effectiveConnectionString = csBuilder.ConnectionString;

            // Fixed the pathing logic to be more robust
            string FindScript(string fileName, string startDir)
            {
                var dir = new DirectoryInfo(startDir);
                for (int i = 0; i < 8 && dir != null; i++)
                {
                    try
                    {
                        var found = Directory.EnumerateFiles(dir.FullName, fileName, SearchOption.AllDirectories).FirstOrDefault();
                        if (found != null) return found;
                    }
                    catch { }
                    dir = dir.Parent;
                }
                return null;
            }

            var settingsDir = Path.GetDirectoryName(settingsPath) ?? Directory.GetCurrentDirectory();
            var sqlPath = FindScript("AddBatchesAndSubmissions.sql", settingsDir);

            if (sqlPath == null)
            {
                Console.Error.WriteLine($"Migration script 'AddBatchesAndSubmissions.sql' not found in search path.");
                return 1;
            }

            try
            {
                var sql = File.ReadAllText(sqlPath);
                using var conn = new MySqlConnection(effectiveConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandTimeout = 0;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                Console.WriteLine("Migration script executed successfully.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Execution failed: " + ex.Message);
                return 1;
            }
        }
    }
}