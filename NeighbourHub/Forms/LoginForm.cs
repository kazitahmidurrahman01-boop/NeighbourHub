using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.Models;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private ComboBox cmbRole = null!;
        private Button btnLogin = null!;
        private Button btnClear = null!;
        private Button btnExit = null!;
        private Label lblStatus = null!;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "NeighbourHub - Login";
            this.Size = new Size(820, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = ThemeColors.Background;
            this.Font = UIHelper.BodyFont;

            // 1. Left Branding Panel
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 320,
                BackColor = ThemeColors.SidebarDark
            };

            var lblLogo = new Label
            {
                Text = "🏢 NeighbourHub",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(24, 60),
                AutoSize = true
            };

            var lblTagline = new Label
            {
                Text = "Smart Apartment\nManagement System",
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                ForeColor = ThemeColors.PrimaryLight,
                Location = new Point(28, 105),
                AutoSize = true
            };

            var lblDesc = new Label
            {
                Text = "• Role-Based Access Control\n• Comprehensive Rent & Payments\n• Live Complaints & Maintenance\n• Resident Directory & Community\n• Intelligent Reports & Analytics",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(203, 213, 225),
                Location = new Point(28, 190),
                Size = new Size(265, 140)
            };

            var lblVersion = new Label
            {
                Text = "v1.0 LocalDB Desktop Edition",
                Font = UIHelper.SmallFont,
                ForeColor = ThemeColors.TextMuted,
                Location = new Point(28, 430),
                AutoSize = true
            };

            leftPanel.Controls.Add(lblVersion);
            leftPanel.Controls.Add(lblDesc);
            leftPanel.Controls.Add(lblTagline);
            leftPanel.Controls.Add(lblLogo);

            // 2. Right Login Form Panel
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(40, 30, 40, 30)
            };

            var lblWelcome = new Label
            {
                Text = "Sign In to Account",
                Font = UIHelper.HeaderFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(40, 35),
                AutoSize = true
            };

            var lblSub = new Label
            {
                Text = "Enter your credentials and select your role.",
                Font = UIHelper.BodyFont,
                ForeColor = ThemeColors.TextSecondary,
                Location = new Point(42, 70),
                AutoSize = true
            };

            // Username
            var lblUser = new Label
            {
                Text = "Username:",
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(42, 110),
                AutoSize = true
            };

            txtUsername = new TextBox
            {
                Location = new Point(45, 135),
                Size = new Size(390, 32),
                Font = new Font("Segoe UI", 11F)
            };

            // Password
            var lblPass = new Label
            {
                Text = "Password:",
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(42, 180),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(45, 205),
                Size = new Size(390, 32),
                Font = new Font("Segoe UI", 11F),
                PasswordChar = '●'
            };

            // Role
            var lblRole = new Label
            {
                Text = "Role:",
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(42, 250),
                AutoSize = true
            };

            cmbRole = new ComboBox
            {
                Location = new Point(45, 275),
                Size = new Size(390, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5F)
            };
            cmbRole.Items.AddRange(new object[] {
                "Admin",
                "Property Owner",
                "Building Manager",
                "Resident"
            });
            cmbRole.SelectedIndex = 0;

            // Buttons
            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(45, 330),
                Size = new Size(120, 38)
            };
            UIHelper.StyleButton(btnLogin, ThemeColors.Primary, Color.White);
            btnLogin.Click += BtnLogin_Click;

            btnClear = new Button
            {
                Text = "Clear",
                Location = new Point(175, 330),
                Size = new Size(100, 38)
            };
            UIHelper.StyleButton(btnClear, ThemeColors.Secondary, Color.White);
            btnClear.Click += (s, e) => { txtUsername.Clear(); txtPassword.Clear(); txtUsername.Focus(); };

            btnExit = new Button
            {
                Text = "Exit",
                Location = new Point(285, 330),
                Size = new Size(90, 38)
            };
            UIHelper.StyleButton(btnExit, ThemeColors.Danger, Color.White);
            btnExit.Click += (s, e) => Application.Exit();

            // Quick Demo Buttons / Viva helper
            var lblQuick = new Label
            {
                Text = "Quick Demo Logins (Click to prefill):",
                Font = UIHelper.SmallFont,
                ForeColor = ThemeColors.TextSecondary,
                Location = new Point(45, 385),
                AutoSize = true
            };

            var btnQuickAdmin = CreateQuickBtn("Admin", "admin", "admin123", 0, new Point(45, 410));
            var btnQuickOwner = CreateQuickBtn("Owner", "owner1", "owner123", 1, new Point(125, 410));
            var btnQuickManager = CreateQuickBtn("Manager", "manager1", "manager123", 2, new Point(205, 410));
            var btnQuickResident = CreateQuickBtn("Resident", "tanisha", "tenant123", 3, new Point(295, 410));

            lblStatus = new Label
            {
                Text = "",
                ForeColor = ThemeColors.Danger,
                Font = UIHelper.SmallFont,
                Location = new Point(45, 450),
                AutoSize = true
            };

            rightPanel.Controls.Add(lblWelcome);
            rightPanel.Controls.Add(lblSub);
            rightPanel.Controls.Add(lblUser);
            rightPanel.Controls.Add(txtUsername);
            rightPanel.Controls.Add(lblPass);
            rightPanel.Controls.Add(txtPassword);
            rightPanel.Controls.Add(lblRole);
            rightPanel.Controls.Add(cmbRole);
            rightPanel.Controls.Add(btnLogin);
            rightPanel.Controls.Add(btnClear);
            rightPanel.Controls.Add(btnExit);
            rightPanel.Controls.Add(lblQuick);
            rightPanel.Controls.Add(btnQuickAdmin);
            rightPanel.Controls.Add(btnQuickOwner);
            rightPanel.Controls.Add(btnQuickManager);
            rightPanel.Controls.Add(btnQuickResident);
            rightPanel.Controls.Add(lblStatus);

            this.Controls.Add(rightPanel);
            this.Controls.Add(leftPanel);

            this.AcceptButton = btnLogin;
        }

        private Button CreateQuickBtn(string label, string u, string p, int roleIndex, Point loc)
        {
            var btn = new Button
            {
                Text = label,
                Size = new Size(72, 26),
                Location = loc,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeColors.PrimaryLight,
                ForeColor = ThemeColors.PrimaryDark,
                Font = UIHelper.SmallFont,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = ThemeColors.Primary;
            btn.Click += (s, e) =>
            {
                txtUsername.Text = u;
                txtPassword.Text = p;
                cmbRole.SelectedIndex = roleIndex;
                lblStatus.Text = string.Empty;
            };
            return btn;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string selectedRole = cmbRole.SelectedItem?.ToString() ?? string.Empty;

            // Validation
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                UIHelper.ShowWarning("Please fill all required fields (Username and Password).");
                lblStatus.Text = "Username and password cannot be empty.";
                txtUsername.Focus();
                return;
            }

            try
            {
                string query = @"
                    SELECT UserId, Username, Password, FullName, Email, Phone, Role, Status, CreatedAt 
                    FROM dbo.Users 
                    WHERE Username = @Username AND Password = @Password";

                var parameters = new[]
                {
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Password", password)
                };

                var dt = DbHelper.ExecuteDataTable(query, parameters);

                if (dt.Rows.Count == 0)
                {
                    UIHelper.ShowError("Invalid Username or Password.");
                    lblStatus.Text = "Invalid login credentials. Please try again.";
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                var row = dt.Rows[0];
                string dbStatus = row["Status"].ToString() ?? "Active";
                if (!string.Equals(dbStatus, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    UIHelper.ShowError("This user account is inactive. Please contact the administrator.");
                    return;
                }

                string dbRole = row["Role"].ToString() ?? string.Empty;
                if (!string.Equals(dbRole, selectedRole, StringComparison.OrdinalIgnoreCase))
                {
                    UIHelper.ShowWarning($"Role mismatch! Your account role is '{dbRole}', but you selected '{selectedRole}'.");
                    lblStatus.Text = $"Account role is {dbRole}.";
                    return;
                }

                // Construct User model and set Session
                var user = new User
                {
                    UserId = Convert.ToInt32(row["UserId"]),
                    Username = row["Username"].ToString() ?? string.Empty,
                    Password = row["Password"].ToString() ?? string.Empty,
                    FullName = row["FullName"].ToString() ?? string.Empty,
                    Email = row["Email"]?.ToString(),
                    Phone = row["Phone"]?.ToString(),
                    Role = dbRole,
                    Status = dbStatus,
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                };

                SessionManager.SetSession(user);

                // Open appropriate Dashboard
                Form dashboardForm = user.Role switch
                {
                    "Admin" => new AdminDashboard(),
                    "Property Owner" => new PropertyOwnerDashboard(),
                    "Building Manager" => new ManagerDashboard(),
                    "Resident" => new ResidentDashboard(),
                    _ => throw new Exception("Unrecognized user role.")
                };

                this.Hide();
                txtPassword.Clear();
                lblStatus.Text = string.Empty;

                dashboardForm.FormClosed += (s, args) =>
                {
                    SessionManager.Logout();
                    this.Show();
                    txtUsername.Focus();
                };

                dashboardForm.Show();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Unable to connect to database.\nDetails: {ex.Message}");
            }
        }
    }
}
