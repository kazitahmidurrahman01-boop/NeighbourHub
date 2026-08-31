using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class UserManagementForm : Form
    {
        private int selectedUserId = 0;
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private TextBox txtFullName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPhone = null!;
        private ComboBox cmbRole = null!;
        private ComboBox cmbStatus = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbRoleFilter = null!;
        private DataGridView dgvUsers = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public UserManagementForm()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Text = "User Management - NeighbourHub";
            this.Size = new Size(1000, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = ThemeColors.Background;
            this.Font = UIHelper.BodyFont;

            // Top Header Panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ThemeColors.HeaderDark,
                Padding = new Padding(20, 15, 20, 15)
            };

            var lblHeader = new Label
            {
                Text = "👥 User Management (System Users & Roles)",
                Font = UIHelper.SubheaderFont,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 16)
            };
            headerPanel.Controls.Add(lblHeader);

            // Left Input Controls Panel
            var inputPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 340,
                BackColor = Color.White,
                Padding = new Padding(16)
            };

            int top = 16;
            inputPanel.Controls.Add(CreateLabel("Username *:", 16, top));
            txtUsername = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtUsername);
            top += 58;

            inputPanel.Controls.Add(CreateLabel("Password *:", 16, top));
            txtPassword = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtPassword);
            top += 58;

            inputPanel.Controls.Add(CreateLabel("Full Name *:", 16, top));
            txtFullName = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtFullName);
            top += 58;

            inputPanel.Controls.Add(CreateLabel("Email Address:", 16, top));
            txtEmail = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtEmail);
            top += 58;

            inputPanel.Controls.Add(CreateLabel("Phone Number:", 16, top));
            txtPhone = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtPhone);
            top += 58;

            inputPanel.Controls.Add(CreateLabel("Role *:", 16, top));
            cmbRole = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new object[] { "Admin", "Property Owner", "Building Manager", "Resident" });
            cmbRole.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbRole);
            top += 58;

            inputPanel.Controls.Add(CreateLabel("Account Status *:", 16, top));
            cmbStatus = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            cmbStatus.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbStatus);
            top += 62;

            // Action Buttons
            btnAdd = new Button { Text = "➕ Add User", Location = new Point(16, top), Size = new Size(140, 36) };
            UIHelper.StyleButton(btnAdd, ThemeColors.Primary, Color.White);
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button { Text = "💾 Update", Location = new Point(165, top), Size = new Size(150, 36) };
            UIHelper.StyleButton(btnUpdate, ThemeColors.Success, Color.White);
            btnUpdate.Click += BtnUpdate_Click;
            top += 44;

            btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(16, top), Size = new Size(140, 36) };
            UIHelper.StyleButton(btnDelete, ThemeColors.Danger, Color.White);
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "🔄 Clear", Location = new Point(165, top), Size = new Size(150, 36) };
            UIHelper.StyleButton(btnClear, ThemeColors.Secondary, Color.White);
            btnClear.Click += (s, e) => ClearForm();

            inputPanel.Controls.Add(btnAdd);
            inputPanel.Controls.Add(btnUpdate);
            inputPanel.Controls.Add(btnDelete);
            inputPanel.Controls.Add(btnClear);

            // Right Panel (Search & DataGridView)
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Background,
                Padding = new Padding(16)
            };

            // Search Bar Sub-Panel
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblSearch = new Label { Text = "Search:", Location = new Point(12, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(70, 11), Size = new Size(200, 28) };
            txtSearch.TextChanged += (s, e) => FilterUsers();

            var lblFilterRole = new Label { Text = "Filter Role:", Location = new Point(290, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbRoleFilter = new ComboBox { Location = new Point(370, 11), Size = new Size(150, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRoleFilter.Items.AddRange(new object[] { "All Roles", "Admin", "Property Owner", "Building Manager", "Resident" });
            cmbRoleFilter.SelectedIndex = 0;
            cmbRoleFilter.SelectedIndexChanged += (s, e) => FilterUsers();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(535, 9), Size = new Size(80, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbRoleFilter.SelectedIndex = 0; LoadUsers(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblFilterRole);
            searchPanel.Controls.Add(cmbRoleFilter);
            searchPanel.Controls.Add(btnRefresh);

            // Grid
            dgvUsers = new DataGridView
            {
                Dock = DockStyle.Fill
            };
            UIHelper.StyleDataGridView(dgvUsers);
            dgvUsers.CellClick += DgvUsers_CellClick;

            rightPanel.Controls.Add(dgvUsers);
            rightPanel.Controls.Add(searchPanel);

            this.Controls.Add(rightPanel);
            this.Controls.Add(inputPanel);
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

        private void LoadUsers()
        {
            try
            {
                string query = "SELECT UserId, Username, FullName, Role, Email, Phone, Status, CreatedAt FROM dbo.Users ORDER BY UserId DESC";
                var dt = DbHelper.ExecuteDataTable(query);
                dgvUsers.DataSource = dt;

                if (dgvUsers.Columns["UserId"] != null) dgvUsers.Columns["UserId"].Width = 60;
                if (dgvUsers.Columns["Username"] != null) dgvUsers.Columns["Username"].Width = 100;
                if (dgvUsers.Columns["FullName"] != null) dgvUsers.Columns["FullName"].Width = 140;
                if (dgvUsers.Columns["Role"] != null) dgvUsers.Columns["Role"].Width = 120;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load users: {ex.Message}");
            }
        }

        private void FilterUsers()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string selectedRole = cmbRoleFilter.SelectedItem?.ToString() ?? "All Roles";

                string query = "SELECT UserId, Username, FullName, Role, Email, Phone, Status, CreatedAt FROM dbo.Users WHERE 1=1 ";
                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (Username LIKE @Keyword OR FullName LIKE @Keyword OR Email LIKE @Keyword OR Phone LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                if (selectedRole != "All Roles")
                {
                    query += " AND Role = @Role ";
                    parameters.Add(new SqlParameter("@Role", selectedRole));
                }

                query += " ORDER BY UserId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvUsers.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvUsers_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvUsers.Rows.Count)
            {
                var row = dgvUsers.Rows[e.RowIndex];
                selectedUserId = Convert.ToInt32(row.Cells["UserId"].Value);
                txtUsername.Text = row.Cells["Username"].Value?.ToString() ?? string.Empty;
                txtFullName.Text = row.Cells["FullName"].Value?.ToString() ?? string.Empty;
                txtEmail.Text = row.Cells["Email"].Value?.ToString() ?? string.Empty;
                txtPhone.Text = row.Cells["Phone"].Value?.ToString() ?? string.Empty;

                string role = row.Cells["Role"].Value?.ToString() ?? "Resident";
                cmbRole.SelectedItem = role;

                string status = row.Cells["Status"].Value?.ToString() ?? "Active";
                cmbStatus.SelectedItem = status;

                // Load existing password for editing if needed
                var pwdObj = DbHelper.ExecuteScalar("SELECT Password FROM dbo.Users WHERE UserId = @UserId",
                    new[] { new SqlParameter("@UserId", selectedUserId) });
                txtPassword.Text = pwdObj?.ToString() ?? string.Empty;
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                // Check duplicate username
                string checkQuery = "SELECT COUNT(*) FROM dbo.Users WHERE Username = @Username";
                int count = Convert.ToInt32(DbHelper.ExecuteScalar(checkQuery, new[] { new SqlParameter("@Username", txtUsername.Text.Trim()) }));
                if (count > 0)
                {
                    UIHelper.ShowWarning("This username already exists. Please choose a unique username.");
                    return;
                }

                string insertQuery = @"
                    INSERT INTO dbo.Users (Username, Password, FullName, Email, Phone, Role, Status)
                    VALUES (@Username, @Password, @FullName, @Email, @Phone, @Role, @Status)";

                var parameters = new[]
                {
                    new SqlParameter("@Username", txtUsername.Text.Trim()),
                    new SqlParameter("@Password", txtPassword.Text.Trim()),
                    new SqlParameter("@FullName", txtFullName.Text.Trim()),
                    new SqlParameter("@Email", (object?)txtEmail.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Phone", (object?)txtPhone.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Role", cmbRole.SelectedItem!.ToString()),
                    new SqlParameter("@Status", cmbStatus.SelectedItem!.ToString())
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Data Saved Successfully!");
                ClearForm();
                LoadUsers();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error adding user: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedUserId <= 0)
            {
                UIHelper.ShowWarning("Please select a user from the table to update.");
                return;
            }

            if (!ValidateInputs()) return;

            try
            {
                // Check duplicate username for other users
                string checkQuery = "SELECT COUNT(*) FROM dbo.Users WHERE Username = @Username AND UserId <> @UserId";
                int count = Convert.ToInt32(DbHelper.ExecuteScalar(checkQuery, new[] {
                    new SqlParameter("@Username", txtUsername.Text.Trim()),
                    new SqlParameter("@UserId", selectedUserId)
                }));

                if (count > 0)
                {
                    UIHelper.ShowWarning("Another user already exists with this username.");
                    return;
                }

                string updateQuery = @"
                    UPDATE dbo.Users 
                    SET Username = @Username, Password = @Password, FullName = @FullName, 
                        Email = @Email, Phone = @Phone, Role = @Role, Status = @Status
                    WHERE UserId = @UserId";

                var parameters = new[]
                {
                    new SqlParameter("@Username", txtUsername.Text.Trim()),
                    new SqlParameter("@Password", txtPassword.Text.Trim()),
                    new SqlParameter("@FullName", txtFullName.Text.Trim()),
                    new SqlParameter("@Email", (object?)txtEmail.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Phone", (object?)txtPhone.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Role", cmbRole.SelectedItem!.ToString()),
                    new SqlParameter("@Status", cmbStatus.SelectedItem!.ToString()),
                    new SqlParameter("@UserId", selectedUserId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Data Updated Successfully!");
                ClearForm();
                LoadUsers();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating user: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedUserId <= 0)
            {
                UIHelper.ShowWarning("Please select a user to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this user? This action cannot be undone."))
            {
                return;
            }

            try
            {
                string deleteQuery = "DELETE FROM dbo.Users WHERE UserId = @UserId";
                DbHelper.ExecuteNonQuery(deleteQuery, new[] { new SqlParameter("@UserId", selectedUserId) });

                UIHelper.ShowSuccess("Data Deleted Successfully!");
                ClearForm();
                LoadUsers();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Cannot delete user. It may be linked to properties, flats, or records.\nDetails: {ex.Message}");
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                UIHelper.ShowWarning("Please fill all required fields (Username, Password, Full Name).");
                return false;
            }
            return true;
        }

        private void ClearForm()
        {
            selectedUserId = 0;
            txtUsername.Clear();
            txtPassword.Clear();
            txtFullName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            cmbRole.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
        }
    }
}
