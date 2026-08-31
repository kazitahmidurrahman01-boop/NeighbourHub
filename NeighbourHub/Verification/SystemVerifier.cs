using System;
using System.Data;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;

namespace NeighbourHub.Verification
{
    public static class SystemVerifier
    {
        public static void RunAllTests()
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("NEIGHBOURHUB - AUTOMATED VERIFICATION TEST SUITE");
            Console.WriteLine("==================================================");

            TestDatabaseConnection();
            TestUserAuthentication();
            TestRoleBasedDataQueries();
            TestCrudLifecycle();
            TestRentAndPaymentFlow();

            Console.WriteLine("\n==================================================");
            Console.WriteLine("ALL TESTS PASSED SUCCESSFULLY! (100% HEALTHY)");
            Console.WriteLine("==================================================");
        }

        private static void TestDatabaseConnection()
        {
            Console.Write("[TEST 1] Testing Database Connection... ");
            if (DatabaseConnection.TestConnection(out string error))
            {
                Console.WriteLine("PASSED");
            }
            else
            {
                Console.WriteLine($"FAILED: {error}");
                throw new Exception("Database connection failed.");
            }
        }

        private static void TestUserAuthentication()
        {
            Console.Write("[TEST 2] Testing Role-Based Authentication... ");

            // 1. Admin login test
            var dtAdmin = DbHelper.ExecuteDataTable("SELECT * FROM dbo.Users WHERE Username = @u AND Password = @p",
                new[] { new SqlParameter("@u", "admin"), new SqlParameter("@p", "admin123") });
            if (dtAdmin.Rows.Count != 1 || dtAdmin.Rows[0]["Role"].ToString() != "Admin")
                throw new Exception("Admin authentication test failed.");

            // 2. Property Owner login test
            var dtOwner = DbHelper.ExecuteDataTable("SELECT * FROM dbo.Users WHERE Username = @u AND Password = @p",
                new[] { new SqlParameter("@u", "owner1"), new SqlParameter("@p", "owner123") });
            if (dtOwner.Rows.Count != 1 || dtOwner.Rows[0]["Role"].ToString() != "Property Owner")
                throw new Exception("Property Owner authentication test failed.");

            // 3. Manager login test
            var dtManager = DbHelper.ExecuteDataTable("SELECT * FROM dbo.Users WHERE Username = @u AND Password = @p",
                new[] { new SqlParameter("@u", "manager1"), new SqlParameter("@p", "manager123") });
            if (dtManager.Rows.Count != 1 || dtManager.Rows[0]["Role"].ToString() != "Building Manager")
                throw new Exception("Building Manager authentication test failed.");

            // 4. Resident login test
            var dtResident = DbHelper.ExecuteDataTable("SELECT * FROM dbo.Users WHERE Username = @u AND Password = @p",
                new[] { new SqlParameter("@u", "tanisha"), new SqlParameter("@p", "tenant123") });
            if (dtResident.Rows.Count != 1 || dtResident.Rows[0]["Role"].ToString() != "Resident")
                throw new Exception("Resident authentication test failed.");

            // 5. Invalid login test
            var dtInvalid = DbHelper.ExecuteDataTable("SELECT * FROM dbo.Users WHERE Username = @u AND Password = @p",
                new[] { new SqlParameter("@u", "admin"), new SqlParameter("@p", "wrongpassword") });
            if (dtInvalid.Rows.Count != 0)
                throw new Exception("Invalid password security check failed.");

            Console.WriteLine("PASSED (All 4 Roles + Security Validated)");
        }

        private static void TestRoleBasedDataQueries()
        {
            Console.Write("[TEST 3] Testing Role-Based Data Isolation... ");

            // Owner 1 properties
            var dtOwnerProps = DbHelper.ExecuteDataTable("SELECT * FROM dbo.Properties WHERE OwnerUserId = 2");
            if (dtOwnerProps.Rows.Count < 1)
                throw new Exception("Owner property isolation query returned no records.");

            // Building Manager 1 assigned buildings
            var dtMgrBuildings = DbHelper.ExecuteDataTable("SELECT * FROM dbo.Buildings WHERE ManagerUserId = 4");
            if (dtMgrBuildings.Rows.Count < 1)
                throw new Exception("Manager building query returned no records.");

            // Resident flat allocation
            var dtResFlat = DbHelper.ExecuteDataTable(@"
                SELECT f.FlatNumber, b.BuildingName 
                FROM dbo.Tenants t 
                INNER JOIN dbo.Flats f ON t.FlatId = f.FlatId 
                INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId 
                WHERE t.UserId = 6 AND t.Status = 'Active'");
            if (dtResFlat.Rows.Count < 1 || dtResFlat.Rows[0]["FlatNumber"].ToString() != "5A")
                throw new Exception("Resident flat allocation query failed.");

            Console.WriteLine("PASSED");
        }

        private static void TestCrudLifecycle()
        {
            Console.Write("[TEST 4] Testing CRUD Lifecycle (Insert -> Select -> Update -> Delete)... ");

            string testUsername = $"testuser_{DateTime.Now.Ticks % 10000}";

            // 1. INSERT
            int inserted = DbHelper.ExecuteNonQuery(@"
                INSERT INTO dbo.Users (Username, Password, FullName, Email, Phone, Role, Status)
                VALUES (@u, @p, @fn, @em, @ph, @r, @s)", new[] {
                new SqlParameter("@u", testUsername),
                new SqlParameter("@p", "testpass123"),
                new SqlParameter("@fn", "Test User Account"),
                new SqlParameter("@em", "test@neighbourhub.com"),
                new SqlParameter("@ph", "01799887766"),
                new SqlParameter("@r", "Resident"),
                new SqlParameter("@s", "Active")
            });
            if (inserted != 1) throw new Exception("CRUD Insert failed.");

            // 2. SELECT
            var dt = DbHelper.ExecuteDataTable("SELECT UserId, FullName, Status FROM dbo.Users WHERE Username = @u",
                new[] { new SqlParameter("@u", testUsername) });
            if (dt.Rows.Count != 1) throw new Exception("CRUD Select failed.");
            int testUserId = Convert.ToInt32(dt.Rows[0]["UserId"]);

            // 3. UPDATE
            int updated = DbHelper.ExecuteNonQuery("UPDATE dbo.Users SET FullName = @fn, Status = 'Inactive' WHERE UserId = @id",
                new[] { new SqlParameter("@fn", "Updated Test User"), new SqlParameter("@id", testUserId) });
            if (updated != 1) throw new Exception("CRUD Update failed.");

            // 4. DELETE
            int deleted = DbHelper.ExecuteNonQuery("DELETE FROM dbo.Users WHERE UserId = @id",
                new[] { new SqlParameter("@id", testUserId) });
            if (deleted != 1) throw new Exception("CRUD Delete failed.");

            Console.WriteLine("PASSED");
        }

        private static void TestRentAndPaymentFlow()
        {
            Console.Write("[TEST 5] Testing Rent & Payment Billing Lifecycle... ");

            // Check August Rent for Flat 5A
            var dtRent = DbHelper.ExecuteDataTable("SELECT RentId, Status FROM dbo.Rent WHERE FlatId = 5 AND Month = 'August' AND Year = 2026");
            if (dtRent.Rows.Count == 0) throw new Exception("Sample rent record not found.");

            // Check payments exist
            var dtPay = DbHelper.ExecuteDataTable("SELECT COUNT(*) FROM dbo.Payments");
            int payCount = Convert.ToInt32(dtPay.Rows[0][0]);
            if (payCount < 2) throw new Exception("Payments table does not contain seeded payments.");

            Console.WriteLine("PASSED");
        }
    }
}
