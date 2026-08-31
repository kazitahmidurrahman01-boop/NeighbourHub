using System;
using System.IO;
using Microsoft.Data.SqlClient;

namespace NeighbourHub.Data
{
    /// <summary>
    /// Self-healing Database Initializer.
    /// Checks if SQL Server LocalDB database and tables exist on application startup,
    /// creating and seeding them if necessary.
    /// </summary>
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            try
            {
                // 1. Check master connection and ensure database exists
                using (var masterConn = new SqlConnection(DatabaseConnection.MasterConnectionString))
                {
                    masterConn.Open();
                    using (var checkDbCmd = new SqlCommand("SELECT database_id FROM sys.databases WHERE name = 'NeighbourHubDB'", masterConn))
                    {
                        var result = checkDbCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            using (var createDbCmd = new SqlCommand("CREATE DATABASE [NeighbourHubDB]", masterConn))
                            {
                                createDbCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                // 2. Check if Users table exists; if not, run schema and seed script
                using (var appConn = DatabaseConnection.GetConnection())
                {
                    appConn.Open();
                    bool tablesExist = false;
                    using (var checkTableCmd = new SqlCommand("SELECT OBJECT_ID('dbo.Users', 'U')", appConn))
                    {
                        var tblResult = checkTableCmd.ExecuteScalar();
                        tablesExist = (tblResult != null && tblResult != DBNull.Value);
                    }

                    if (!tablesExist)
                    {
                        string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "NeighbourHubDB_Schema_And_Seed.sql");
                        if (!File.Exists(scriptPath))
                        {
                            // Try navigating up from bin folder to project folder
                            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Database\NeighbourHubDB_Schema_And_Seed.sql"));
                            if (File.Exists(projectRoot))
                            {
                                scriptPath = projectRoot;
                            }
                        }

                        if (File.Exists(scriptPath))
                        {
                            string sql = File.ReadAllText(scriptPath);
                            ExecuteSqlScript(appConn, sql);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Initialization note: {ex.Message}");
            }
        }

        private static void ExecuteSqlScript(SqlConnection connection, string fullScript)
        {
            string[] commands = fullScript.Split(new[] { "\nGO\r", "\nGO\n", "\r\nGO\r\n", "\nGO", "\r\nGO" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var cmdText in commands)
            {
                string trimmed = cmdText.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    try
                    {
                        using (var cmd = new SqlCommand(trimmed, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"SQL command execution note: {ex.Message}");
                    }
                }
            }
        }
    }
}
