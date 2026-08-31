using System;
using Microsoft.Data.SqlClient;

namespace NeighbourHub.Data
{
    /// <summary>
    /// Central Database Connection Manager using ADO.NET.
    /// Provides consistent connection configuration for SQL Server LocalDB.
    /// </summary>
    public class DatabaseConnection
    {
        // Connection string for Microsoft SQL Server LocalDB
        private static readonly string ConnectionString = 
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NeighbourHubDB;Integrated Security=True;TrustServerCertificate=True;";

        // Master connection string used when initializing or checking database existence
        public static readonly string MasterConnectionString = 
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True;";

        /// <summary>
        /// Gets a new SQL Server connection to NeighbourHubDB.
        /// </summary>
        /// <returns>SqlConnection instance</returns>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        /// <summary>
        /// Returns the configured connection string.
        /// </summary>
        public static string GetConnectionString()
        {
            return ConnectionString;
        }

        /// <summary>
        /// Tests the database connection and returns true if successful.
        /// </summary>
        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
