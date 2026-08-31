using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace NeighbourHub.Data
{
    /// <summary>
    /// ADO.NET Helper class for clean, secure, parameterized database operations.
    /// Eliminates SQL injection and provides convenient methods for DataTables and CRUD.
    /// </summary>
    public static class DbHelper
    {
        /// <summary>
        /// Executes a SELECT query with parameters and returns results in a DataTable.
        /// </summary>
        public static DataTable ExecuteDataTable(string query, SqlParameter[]? parameters = null)
        {
            var dataTable = new DataTable();
            using (var connection = DatabaseConnection.GetConnection())
            {
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            return dataTable;
        }

        /// <summary>
        /// Executes an INSERT, UPDATE, or DELETE query with parameters.
        /// </summary>
        public static int ExecuteNonQuery(string query, SqlParameter[]? parameters = null)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Executes a query that returns a single scalar value (e.g. COUNT, MAX, or single column).
        /// </summary>
        public static object? ExecuteScalar(string query, SqlParameter[]? parameters = null)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    return command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Helper to create a SqlParameter easily.
        /// </summary>
        public static SqlParameter CreateParam(string name, object? value, SqlDbType dbType)
        {
            var param = new SqlParameter(name, dbType);
            param.Value = value ?? DBNull.Value;
            return param;
        }
    }
}
