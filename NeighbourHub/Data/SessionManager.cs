using System;
using System.Data;
using Microsoft.Data.SqlClient;
using NeighbourHub.Models;

namespace NeighbourHub.Data
{
    /// <summary>
    /// Manages the currently logged-in user session and active context.
    /// </summary>
    public static class SessionManager
    {
        public static User? CurrentUser { get; private set; }

        // Context details populated upon login
        public static int? ResidentFlatId { get; private set; }
        public static string? ResidentFlatNumber { get; private set; }
        public static int? ResidentBuildingId { get; private set; }
        public static string? ResidentBuildingName { get; private set; }
        public static int? ManagerBuildingId { get; private set; }
        public static string? ManagerBuildingName { get; private set; }

        public static bool IsLoggedIn => CurrentUser != null;

        public static void SetSession(User user)
        {
            CurrentUser = user;
            LoadUserContext();
        }

        public static void Logout()
        {
            CurrentUser = null;
            ResidentFlatId = null;
            ResidentFlatNumber = null;
            ResidentBuildingId = null;
            ResidentBuildingName = null;
            ManagerBuildingId = null;
            ManagerBuildingName = null;
        }

        /// <summary>
        /// Loads contextual IDs for quick filtering in Resident / Manager / Owner views.
        /// </summary>
        private static void LoadUserContext()
        {
            if (CurrentUser == null) return;

            if (CurrentUser.Role == "Resident")
            {
                string query = @"
                    SELECT r.FlatId, f.FlatNumber, f.BuildingId, b.BuildingName
                    FROM dbo.Residents r
                    INNER JOIN dbo.Flats f ON r.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    WHERE r.UserId = @UserId";

                var dt = DbHelper.ExecuteDataTable(query, new[] {
                    new SqlParameter("@UserId", CurrentUser.UserId)
                });

                if (dt.Rows.Count > 0)
                {
                    ResidentFlatId = Convert.ToInt32(dt.Rows[0]["FlatId"]);
                    ResidentFlatNumber = dt.Rows[0]["FlatNumber"].ToString();
                    ResidentBuildingId = Convert.ToInt32(dt.Rows[0]["BuildingId"]);
                    ResidentBuildingName = dt.Rows[0]["BuildingName"].ToString();
                }
            }
            else if (CurrentUser.Role == "Building Manager")
            {
                string query = @"
                    SELECT BuildingId, BuildingName
                    FROM dbo.Buildings
                    WHERE ManagerUserId = @UserId";

                var dt = DbHelper.ExecuteDataTable(query, new[] {
                    new SqlParameter("@UserId", CurrentUser.UserId)
                });

                if (dt.Rows.Count > 0)
                {
                    ManagerBuildingId = Convert.ToInt32(dt.Rows[0]["BuildingId"]);
                    ManagerBuildingName = dt.Rows[0]["BuildingName"].ToString();
                }
            }
        }
    }
}
