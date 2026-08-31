using System;
using System.Linq;
using System.Windows.Forms;
using NeighbourHub.Data;
using NeighbourHub.Forms;
using NeighbourHub.Verification;

namespace NeighbourHub
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the NeighbourHub Windows desktop application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            // Initialize Windows Forms configuration & High DPI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Ensure SQL Server LocalDB and tables exist
                DatabaseInitializer.InitializeDatabase();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database init note: {ex.Message}");
            }

            // If --verify or --test flag is passed, execute automated tests
            if (args != null && args.Any(a => a.Equals("--verify", StringComparison.OrdinalIgnoreCase) || a.Equals("--test", StringComparison.OrdinalIgnoreCase)))
            {
                SystemVerifier.RunAllTests();
                return;
            }

            // Launch the application with LoginForm
            Application.Run(new LoginForm());
        }
    }
}