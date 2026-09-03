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
        private TextBox txtEmail = null!;
        private TextBox txtPhone = null!;
        private ComboBox cmbRole = null!;
        private Label lblStatus = null!;

        public RegistrationForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "NeighbourHub - New Account Registration";
            this.Size = new Size(540, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = UIHelper.BodyFont;

            // Header strip
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = ThemeColors.SidebarDark
            };
            var lblTitle = new Label
            {
                Text = "🏢 NeighbourHub — Create Account",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(24, 14),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "Fill in your details to register a new account.",
                Font = UIHelper.SmallFont,
                ForeColor = ThemeColors.PrimaryLight,
                Location = new Point(26, 48),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblSub);
            headerPanel.Controls.Add(lblTitle);

            // Form fields
            int left = 40;
            int fieldWidth = 440;

            var lblFN = MakeLabel("Full Name:", left, 105);
            txtFullName = MakeTextBox(left, 128, fieldWidth);

            var lblUN = MakeLabel("Username:", left, 168);
            txtUsername = MakeTextBox(left, 191, fieldWidth);

            var lblPW = MakeLabel("Password:", left, 231);
            txtPassword = MakeTextBox(left, 254, fieldWidth, isPassword: true);

            var lblCPW = MakeLabel("Confirm Password:", left, 294);
            txtConfirmPassword = MakeTextBox(left, 317, fieldWidth, isPassword: true);

            var lblEM = MakeLabel("Email Address:", left, 357);
            txtEmail = MakeTextBox(left, 380, fieldWidth);

            var lblPH = MakeLabel("Phone Number:", left, 420);
            txtPhone = MakeTextBox(left, 443, fieldWidth);

            var lblRole = MakeLabel("Register as (Role):", left, 483);
            cmbRole = new ComboBox
            {
                Location = new Point(left, 506),
                Size = new Size(fieldWidth, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5F)
            };
            cmbRole.Items.AddRange(new object[]
            {
                "Resident",
                "Building Manager",
                "Property Owner"
            });
            cmbRole.SelectedIndex = 0;

            // Note about Admin
            var lblNote = new Label
            {
                Text = "ℹ️ Admin accounts are created only by the System Administrator.",
                Font = UIHelper.SmallFont,
                ForeColor = ThemeColors.TextMuted,
                Location = new Point(left, 540),
                Size = new Size(fieldWidth, 20)
            };

            // Buttons
            var btnRegister = new Button
            {
                Text = "✅ Create Account",
                Location = new Point(left, 572),
                Size = new Size(165, 38)
            };
            UIHelper.StyleButton(btnRegister, ThemeColors.Success, Color.White);
            btnRegister.Click += BtnRegister_Click;

            var btnCancel = new Button
            {
                Text = "✖ Cancel",
                Location = new Point(220, 572),
                Size = new Size(110, 38)
            };
            UIHelper.StyleButton(btnCancel, ThemeColors.Danger, Color.White);
            btnCancel.Click += (s, e) => this.Close();

            lblStatus = new Label
            {
                Text = "",
                ForeColor = ThemeColors.Danger,
                Font = UIHelper.SmallFont,
                Location = new Point(left, 618),
                Size = new Size(fieldWidth, 20)
            };

            this.Controls.Add(headerPanel);
            this.Controls.Add(lblFN);
            this.Controls.Add(txtFullName);
            this.Controls.Add(lblUN);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPW);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblCPW);
            this.Controls.Add(txtConfirmPassword);
            this.Controls.Add(lblEM);
            this.Controls.Add(txtEmail);
            this.Controls.Add(lblPH);
            this.Controls.Add(txtPhone);
            this.Controls.Add(lblRole);
            this.Controls.Add(cmbRole);
            this.Controls.Add(lblNote);
            this.Controls.Add(btnRegister);
            this.Controls.Add(btnCancel);
            this.Controls.Add(lblStatus);
        }

        private Label MakeLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private TextBox MakeTextBox(int x, int y, int width, bool isPassword = false)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 32),
                Font = new Font("Segoe UI", 11F),
                PasswordChar = isPassword ? '●' : '\0'
            };
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            lblStatus.Text = "";
            lblStatus.ForeColor = ThemeColors.Danger;

            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string role = cmbRole.SelectedItem?.ToString() ?? "Resident";

            // Validation
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblStatus.Text = "Full Name, Username and Password are required.";
                return;
            }

            if (username.Length < 4)
            {
                lblStatus.Text = "Username must be at least 4 characters long.";
                return;
            }

            if (password.Length < 6)
            {
                lblStatus.Text = "Password must be at least 6 characters long.";
                return;
            }

            if (password != confirmPassword)
            {
                lblStatus.Text = "Passwords do not match. Please re-enter.";
                txtConfirmPassword.Clear();
                txtConfirmPassword.Focus();
                return;
            }

            if (!string.IsNullOrEmpty(email) && !email.Contains("@"))
            {
                lblStatus.Text = "Please enter a valid email address.";
                return;
            }

            try
            {
                // Check if username already exists
                object? existing = DbHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM dbo.Users WHERE Username = @u",
                    new[] { new SqlParameter("@u", username) });

                if (Convert.ToInt32(existing) > 0)
                {
                    lblStatus.Text = "This username is already taken. Please choose another.";
                    txtUsername.Focus();
                    return;
                }

                // Insert new user (Status = Pending for non-Admin roles, awaiting admin approval)
                string insertQuery = @"
                    INSERT INTO dbo.Users (Username, Password, FullName, Email, Phone, Role, Status, CreatedAt)
                    VALUES (@u, @p, @fn, @em, @ph, @r, @s, GETDATE())";

                int inserted = DbHelper.ExecuteNonQuery(insertQuery, new[]
                {
                    new SqlParameter("@u", username),
                    new SqlParameter("@p", password),
                    new SqlParameter("@fn", fullName),
                    new SqlParameter("@em", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email),
                    new SqlParameter("@ph", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone),
                    new SqlParameter("@r", role),
                    new SqlParameter("@s", "Active")
                });

                if (inserted == 1)
                {
                    lblStatus.ForeColor = ThemeColors.Success;
                    lblStatus.Text = "✅ Account created successfully!";

                    MessageBox.Show(
                        $"🎉 Welcome to NeighbourHub, {fullName}!\n\n" +
                        $"Your account has been created successfully.\n\n" +
                        $"Username : {username}\n" +
                        $"Role      : {role}\n\n" +
                        $"You can now log in with your credentials.",
                        "Registration Successful",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Registration failed. Please try again.\nDetails: {ex.Message}");
            }
        }
    }
}
