using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class ProfileForm : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtFullName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPhone = null!;
        private TextBox txtRole = null!;
        private TextBox txtPassword = null!;
        private Button btnSave = null!;

        public ProfileForm()
        {
            InitializeComponent();
            LoadProfileData();
        }

        private void InitializeComponent()
        {
            this.Text = "My Profile - NeighbourHub";
            this.Size = new Size(580, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = UIHelper.BodyFont;

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ThemeColors.HeaderDark,
                Padding = new Padding(20, 15, 20, 15)
            };

            var lblHeader = new Label
            {
                Text = "👤 User Profile & Security Settings",
                Font = UIHelper.SubheaderFont,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 16)
            };
            headerPanel.Controls.Add(lblHeader);

            var formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 20, 30, 20)
            };

            int top = 20;
            formPanel.Controls.Add(CreateLabel("Username (Read-Only):", 30, top));
            txtUsername = new TextBox { Location = new Point(30, top + 22), Size = new Size(500, 28), ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249) };
            formPanel.Controls.Add(txtUsername);
            top += 56;

            formPanel.Controls.Add(CreateLabel("Role (System Assigned):", 30, top));
            txtRole = new TextBox { Location = new Point(30, top + 22), Size = new Size(500, 28), ReadOnly = true, BackColor = Color.FromArgb(241, 245, 249) };
            formPanel.Controls.Add(txtRole);
            top += 56;

            formPanel.Controls.Add(CreateLabel("Full Name *:", 30, top));
            txtFullName = new TextBox { Location = new Point(30, top + 22), Size = new Size(500, 28) };
            formPanel.Controls.Add(txtFullName);
            top += 56;

            formPanel.Controls.Add(CreateLabel("Email Address:", 30, top));
            txtEmail = new TextBox { Location = new Point(30, top + 22), Size = new Size(500, 28) };
            formPanel.Controls.Add(txtEmail);
            top += 56;

            formPanel.Controls.Add(CreateLabel("Phone Number:", 30, top));
            txtPhone = new TextBox { Location = new Point(30, top + 22), Size = new Size(500, 28) };
            formPanel.Controls.Add(txtPhone);
            top += 56;

            formPanel.Controls.Add(CreateLabel("Password (Change if needed):", 30, top));
            txtPassword = new TextBox { Location = new Point(30, top + 22), Size = new Size(500, 28), PasswordChar = '●' };
            formPanel.Controls.Add(txtPassword);
            top += 62;

            btnSave = new Button { Text = "💾 Save Profile Changes", Location = new Point(30, top), Size = new Size(200, 38) };
            UIHelper.StyleButton(btnSave, ThemeColors.Primary, Color.White);
            btnSave.Click += BtnSave_Click;

            formPanel.Controls.Add(btnSave);

            this.Controls.Add(formPanel);
            this.Controls.Add(headerPanel);
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.TextPrimary
            };
        }

        private void LoadProfileData()
        {
            if (SessionManager.CurrentUser == null) return;

            txtUsername.Text = SessionManager.CurrentUser.Username;
            txtFullName.Text = SessionManager.CurrentUser.FullName;
            txtEmail.Text = SessionManager.CurrentUser.Email ?? string.Empty;
            txtPhone.Text = SessionManager.CurrentUser.Phone ?? string.Empty;
            txtRole.Text = SessionManager.CurrentUser.Role;
            txtPassword.Text = SessionManager.CurrentUser.Password;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                UIHelper.ShowWarning("Full Name and Password cannot be empty.");
                return;
            }

            try
            {
                int userId = SessionManager.CurrentUser!.UserId;
                string updateQuery = @"
                    UPDATE dbo.Users 
                    SET FullName = @FullName, Email = @Email, Phone = @Phone, Password = @Password
                    WHERE UserId = @UserId";

                var parameters = new[]
                {
                    new SqlParameter("@FullName", txtFullName.Text.Trim()),
                    new SqlParameter("@Email", (object?)txtEmail.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Phone", (object?)txtPhone.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Password", txtPassword.Text.Trim()),
                    new SqlParameter("@UserId", userId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);

                // Update session object
                SessionManager.CurrentUser.FullName = txtFullName.Text.Trim();
                SessionManager.CurrentUser.Email = txtEmail.Text.Trim();
                SessionManager.CurrentUser.Phone = txtPhone.Text.Trim();
                SessionManager.CurrentUser.Password = txtPassword.Text.Trim();

                UIHelper.ShowSuccess("Profile Updated Successfully!");
                this.Close();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating profile: {ex.Message}");
            }
        }
    }
}
