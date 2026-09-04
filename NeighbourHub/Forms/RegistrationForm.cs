using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class RegistrationForm : Form
    {
        private TextBox txtFullName = null!;
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private TextBox txtConfirmPassword = null!;
        private ComboBox cmbRole = null!;
        private Label lblStatus = null!;

        public RegistrationForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "NeighbourHub - Register";
            this.Size = new Size(420, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = UIHelper.BodyFont;

            int left = 35;
            int fieldW = 330;

            // Title
            var lblTitle = new Label
            {
                Text = "🏢 Create New Account",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(left, 20),
                AutoSize = true
            };

            // Full Name
            var lblName = new Label
            {
                Text = "Full Name",
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(left, 65),
                AutoSize = true
            };
            txtFullName = new TextBox
            {
                Location = new Point(left, 85),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11F),
                PlaceholderText = "Your full name"
            };

            // Username
            var lblUN = new Label
            {
                Text = "Username",
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(left, 130),
                AutoSize = true
            };
            txtUsername = new TextBox
            {
                Location = new Point(left, 150),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11F),
                PlaceholderText = "Choose a username"
            };

            // Password
            var lblPW = new Label
            {
                Text = "Password",
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(left, 195),
                AutoSize = true
            };
            txtPassword = new TextBox
            {
                Location = new Point(left, 215),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11F),
                PasswordChar = '●',
                PlaceholderText = "Min 6 characters"
            };

            // Confirm Password
            var lblCPW = new Label
            {
                Text = "Confirm Password",
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(left, 260),
                AutoSize = true
            };
            txtConfirmPassword = new TextBox
            {
                Location = new Point(left, 280),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11F),
                PasswordChar = '●',
                PlaceholderText = "Re-enter password"
            };

            // Role
            var lblRole = new Label
            {
                Text = "Role",
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(left, 325),
                AutoSize = true
            };
            cmbRole = new ComboBox
            {
                Location = new Point(left, 345),
                Size = new Size(fieldW, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5F)
            };
            cmbRole.Items.AddRange(new object[]
            {
                "Admin",
                "Property Owner",
                "Building Manager",
                "Resident"
            });
            cmbRole.SelectedIndex = 3; // Default: Resident

            // Status label
            lblStatus = new Label
            {
                Text = "",
                ForeColor = ThemeColors.Danger,
                Font = UIHelper.SmallFont,
                Location = new Point(left, 390),
                Size = new Size(fieldW, 18),
                AutoSize = false
            };

            // Buttons
            var btnCreate = new Button
            {
                Text = "✅ Register",
                Location = new Point(left, 412),
                Size = new Size(155, 38)
            };
            UIHelper.StyleButton(btnCreate, ThemeColors.Success, Color.White);
            btnCreate.Click += BtnCreate_Click;

            var btnCancel = new Button
            {
                Text = "✖ Cancel",
                Location = new Point(210, 412),
                Size = new Size(110, 38)
            };
            UIHelper.StyleButton(btnCancel, ThemeColors.Danger, Color.White);
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblName);
            this.Controls.Add(txtFullName);
            this.Controls.Add(lblUN);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPW);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblCPW);
            this.Controls.Add(txtConfirmPassword);
            this.Controls.Add(lblRole);
            this.Controls.Add(cmbRole);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnCreate);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnCreate;
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            lblStatus.ForeColor = ThemeColors.Danger;

            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            string role = cmbRole.SelectedItem?.ToString() ?? "Resident";

            // --- Validation ---
            if (string.IsNullOrEmpty(fullName))
            {
                lblStatus.Text = "❌ Full name is required.";
                txtFullName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                lblStatus.Text = "❌ Username is required.";
                txtUsername.Focus();
                return;
            }

            if (username.Length < 4)
            {
                lblStatus.Text = "❌ Username must be at least 4 characters.";
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                lblStatus.Text = "❌ Password is required.";
                txtPassword.Focus();
                return;
            }

            if (password.Length < 6)
            {
                lblStatus.Text = "❌ Password must be at least 6 characters.";
                txtPassword.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                lblStatus.Text = "❌ Passwords do not match.";
                txtConfirmPassword.Clear();
                txtConfirmPassword.Focus();
                return;
            }

            try
            {
                // Check duplicate username
                object? existing = DbHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM dbo.Users WHERE Username = @u",
                    new[] { new SqlParameter("@u", username) });

                if (Convert.ToInt32(existing) > 0)
                {
                    lblStatus.Text = "❌ Username already taken. Try another.";
                    txtUsername.Focus();
                    return;
                }

                // Insert new user
                int inserted = DbHelper.ExecuteNonQuery(@"
                    INSERT INTO dbo.Users (Username, Password, FullName, Email, Phone, Role, Status, CreatedAt)
                    VALUES (@u, @p, @fn, NULL, NULL, @r, 'Active', GETDATE())",
                    new[]
                    {
                        new SqlParameter("@u", username),
                        new SqlParameter("@p", password),
                        new SqlParameter("@fn", fullName),
                        new SqlParameter("@r", role)
                    });

                if (inserted == 1)
                {
                    MessageBox.Show(
                        $"✅ Account created successfully!\n\n" +
                        $"Name     : {fullName}\n" +
                        $"Username : {username}\n" +
                        $"Role     : {role}\n\n" +
                        $"You can now log in.",
                        "Registration Successful",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ Error: " + ex.Message;
            }
        }
    }
}
