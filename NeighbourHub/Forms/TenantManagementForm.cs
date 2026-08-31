using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class TenantManagementForm : Form
    {
        private int selectedTenantId = 0;
        private ComboBox cmbUser = null!;
        private ComboBox cmbFlat = null!;
        private DateTimePicker dtpLeaseStart = null!;
        private DateTimePicker dtpLeaseEnd = null!;
        private TextBox txtAgreedRent = null!;
        private TextBox txtSecurityDeposit = null!;
        private TextBox txtEmergencyContact = null!;
        private ComboBox cmbStatus = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbStatusFilter = null!;
        private DataGridView dgvTenants = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public TenantManagementForm()
        {
            InitializeComponent();
            LoadDropdowns();
            LoadTenants();
        }

        private void InitializeComponent()
        {
            this.Text = "Tenant Allocation & Lease Management - NeighbourHub";
            this.Size = new Size(1060, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = ThemeColors.Background;
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
                Text = "📝 Tenant Allocation & Lease Contracts",
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
                Width = 350,
                BackColor = Color.White,
                Padding = new Padding(16),
                AutoScroll = true
            };

            int top = 16;
            inputPanel.Controls.Add(CreateLabel("Tenant (Resident User) *:", 16, top));
            cmbUser = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbUser);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Allocated Flat *:", 16, top));
            cmbFlat = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFlat.SelectedIndexChanged += CmbFlat_SelectedIndexChanged;
            inputPanel.Controls.Add(cmbFlat);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Agreed Monthly Rent (৳) *:", 16, top));
            txtAgreedRent = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtAgreedRent);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Security Deposit (৳):", 16, top));
            txtSecurityDeposit = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), Text = "0" };
            inputPanel.Controls.Add(txtSecurityDeposit);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Lease Start Date *:", 16, top));
            dtpLeaseStart = new DateTimePicker { Location = new Point(16, top + 22), Size = new Size(300, 28), Format = DateTimePickerFormat.Short };
            inputPanel.Controls.Add(dtpLeaseStart);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Lease End Date:", 16, top));
            dtpLeaseEnd = new DateTimePicker { Location = new Point(16, top + 22), Size = new Size(300, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddYears(1) };
            inputPanel.Controls.Add(dtpLeaseEnd);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Emergency Contact Details:", 16, top));
            txtEmergencyContact = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. Brother - 01755123456" };
            inputPanel.Controls.Add(txtEmergencyContact);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Lease Status *:", 16, top));
            cmbStatus = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Active", "Terminated" });
            cmbStatus.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbStatus);
            top += 58;

            // Buttons
            btnAdd = new Button { Text = "➕ Allocate Tenant", Location = new Point(16, top), Size = new Size(140, 36) };
            UIHelper.StyleButton(btnAdd, ThemeColors.Primary, Color.White);
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button { Text = "💾 Update Lease", Location = new Point(165, top), Size = new Size(150, 36) };
            UIHelper.StyleButton(btnUpdate, ThemeColors.Success, Color.White);
            btnUpdate.Click += BtnUpdate_Click;
            top += 42;

            btnDelete = new Button { Text = "🗑️ Remove", Location = new Point(16, top), Size = new Size(140, 36) };
            UIHelper.StyleButton(btnDelete, ThemeColors.Danger, Color.White);
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "🔄 Clear", Location = new Point(165, top), Size = new Size(150, 36) };
            UIHelper.StyleButton(btnClear, ThemeColors.Secondary, Color.White);
            btnClear.Click += (s, e) => ClearForm();

            inputPanel.Controls.Add(btnAdd);
            inputPanel.Controls.Add(btnUpdate);
            inputPanel.Controls.Add(btnDelete);
            inputPanel.Controls.Add(btnClear);

            // Right Panel
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Background,
                Padding = new Padding(16)
            };

            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblSearch = new Label { Text = "Search Tenant:", Location = new Point(12, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(115, 11), Size = new Size(180, 28) };
            txtSearch.TextChanged += (s, e) => FilterTenants();

            var lblFilterStatus = new Label { Text = "Status:", Location = new Point(310, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbStatusFilter = new ComboBox { Location = new Point(370, 11), Size = new Size(140, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Active", "Terminated" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => FilterTenants();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(525, 9), Size = new Size(80, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbStatusFilter.SelectedIndex = 0; LoadTenants(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblFilterStatus);
            searchPanel.Controls.Add(cmbStatusFilter);
            searchPanel.Controls.Add(btnRefresh);

            dgvTenants = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvTenants);
            dgvTenants.CellClick += DgvTenants_CellClick;

            rightPanel.Controls.Add(dgvTenants);
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

        private void LoadDropdowns()
        {
            try
            {
                // Users with Resident role
                var dtUsers = DbHelper.ExecuteDataTable("SELECT UserId, FullName + ' (' + Phone + ')' AS UserDisplay FROM dbo.Users WHERE Role = 'Resident' AND Status = 'Active'");
                cmbUser.DataSource = dtUsers;
                cmbUser.DisplayMember = "UserDisplay";
                cmbUser.ValueMember = "UserId";

                // Flats
                string flatQuery = @"
                    SELECT f.FlatId, 'Flat ' + f.FlatNumber + ' - ' + b.BuildingName + ' (Rent: ৳' + CAST(CAST(f.MonthlyRent AS INT) AS NVARCHAR(20)) + ')' AS FlatDisplay, f.MonthlyRent
                    FROM dbo.Flats f
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId";

                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    flatQuery += $" WHERE p.OwnerUserId = {SessionManager.CurrentUser.UserId}";
                }
                else if (SessionManager.CurrentUser?.Role == "Building Manager")
                {
                    flatQuery += $" WHERE b.ManagerUserId = {SessionManager.CurrentUser.UserId}";
                }

                var dtFlats = DbHelper.ExecuteDataTable(flatQuery);
                cmbFlat.DataSource = dtFlats;
                cmbFlat.DisplayMember = "FlatDisplay";
                cmbFlat.ValueMember = "FlatId";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading dropdowns: {ex.Message}");
            }
        }

        private void CmbFlat_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbFlat.SelectedItem is DataRowView drv)
            {
                if (drv.Row.Table.Columns.Contains("MonthlyRent"))
                {
                    txtAgreedRent.Text = drv["MonthlyRent"].ToString();
                }
            }
        }

        private void LoadTenants()
        {
            try
            {
                string query = @"
                    SELECT t.TenantId, u.FullName AS TenantName, u.Phone, f.FlatNumber, b.BuildingName,
                           t.AgreedRent, t.SecurityDeposit, t.LeaseStartDate, t.LeaseEndDate, t.Status,
                           t.EmergencyContact, t.UserId, t.FlatId
                    FROM dbo.Tenants t
                    INNER JOIN dbo.Users u ON t.UserId = u.UserId
                    INNER JOIN dbo.Flats f ON t.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    query += " WHERE p.OwnerUserId = @OwnerId ";
                    parameters.Add(new SqlParameter("@OwnerId", SessionManager.CurrentUser.UserId));
                }
                else if (SessionManager.CurrentUser?.Role == "Building Manager")
                {
                    query += " WHERE b.ManagerUserId = @MgrId ";
                    parameters.Add(new SqlParameter("@MgrId", SessionManager.CurrentUser.UserId));
                }

                query += " ORDER BY t.TenantId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvTenants.DataSource = dt;

                if (dgvTenants.Columns["TenantId"] != null) dgvTenants.Columns["TenantId"].Width = 60;
                if (dgvTenants.Columns["UserId"] != null) dgvTenants.Columns["UserId"].Visible = false;
                if (dgvTenants.Columns["FlatId"] != null) dgvTenants.Columns["FlatId"].Visible = false;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load tenants: {ex.Message}");
            }
        }

        private void FilterTenants()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string selectedStatus = cmbStatusFilter.SelectedItem?.ToString() ?? "All";

                string query = @"
                    SELECT t.TenantId, u.FullName AS TenantName, u.Phone, f.FlatNumber, b.BuildingName,
                           t.AgreedRent, t.SecurityDeposit, t.LeaseStartDate, t.LeaseEndDate, t.Status,
                           t.EmergencyContact, t.UserId, t.FlatId
                    FROM dbo.Tenants t
                    INNER JOIN dbo.Users u ON t.UserId = u.UserId
                    INNER JOIN dbo.Flats f ON t.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    query += " AND p.OwnerUserId = @OwnerId ";
                    parameters.Add(new SqlParameter("@OwnerId", SessionManager.CurrentUser.UserId));
                }
                else if (SessionManager.CurrentUser?.Role == "Building Manager")
                {
                    query += " AND b.ManagerUserId = @MgrId ";
                    parameters.Add(new SqlParameter("@MgrId", SessionManager.CurrentUser.UserId));
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (u.FullName LIKE @Keyword OR u.Phone LIKE @Keyword OR f.FlatNumber LIKE @Keyword OR b.BuildingName LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                if (selectedStatus != "All")
                {
                    query += " AND t.Status = @Status ";
                    parameters.Add(new SqlParameter("@Status", selectedStatus));
                }

                query += " ORDER BY t.TenantId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvTenants.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvTenants_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvTenants.Rows.Count)
            {
                var row = dgvTenants.Rows[e.RowIndex];
                selectedTenantId = Convert.ToInt32(row.Cells["TenantId"].Value);
                txtAgreedRent.Text = row.Cells["AgreedRent"].Value?.ToString() ?? "0";
                txtSecurityDeposit.Text = row.Cells["SecurityDeposit"].Value?.ToString() ?? "0";
                txtEmergencyContact.Text = row.Cells["EmergencyContact"].Value?.ToString() ?? string.Empty;

                if (row.Cells["LeaseStartDate"].Value != DBNull.Value)
                    dtpLeaseStart.Value = Convert.ToDateTime(row.Cells["LeaseStartDate"].Value);
                if (row.Cells["LeaseEndDate"].Value != DBNull.Value)
                    dtpLeaseEnd.Value = Convert.ToDateTime(row.Cells["LeaseEndDate"].Value);

                if (row.Cells["UserId"].Value != DBNull.Value)
                    cmbUser.SelectedValue = Convert.ToInt32(row.Cells["UserId"].Value);

                if (row.Cells["FlatId"].Value != DBNull.Value)
                    cmbFlat.SelectedValue = Convert.ToInt32(row.Cells["FlatId"].Value);

                string status = row.Cells["Status"].Value?.ToString() ?? "Active";
                cmbStatus.SelectedItem = status;
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                int userId = Convert.ToInt32(cmbUser.SelectedValue);
                int flatId = Convert.ToInt32(cmbFlat.SelectedValue);
                string status = cmbStatus.SelectedItem?.ToString() ?? "Active";

                // Insert into Tenants table
                string insertQuery = @"
                    INSERT INTO dbo.Tenants (UserId, FlatId, LeaseStartDate, LeaseEndDate, AgreedRent, SecurityDeposit, EmergencyContact, Status)
                    VALUES (@UserId, @FlatId, @LeaseStartDate, @LeaseEndDate, @AgreedRent, @SecurityDeposit, @EmergencyContact, @Status);
                    SELECT SCOPE_IDENTITY();";

                var parameters = new[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@FlatId", flatId),
                    new SqlParameter("@LeaseStartDate", dtpLeaseStart.Value.Date),
                    new SqlParameter("@LeaseEndDate", dtpLeaseEnd.Value.Date),
                    new SqlParameter("@AgreedRent", decimal.Parse(txtAgreedRent.Text.Trim())),
                    new SqlParameter("@SecurityDeposit", decimal.Parse(txtSecurityDeposit.Text.Trim())),
                    new SqlParameter("@EmergencyContact", (object?)txtEmergencyContact.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Status", status)
                };

                object? newId = DbHelper.ExecuteScalar(insertQuery, parameters);
                int tenantId = Convert.ToInt32(newId);

                // Update Flat status
                string flatStatus = (status == "Active") ? "Occupied" : "Vacant";
                DbHelper.ExecuteNonQuery("UPDATE dbo.Flats SET Status = @Status WHERE FlatId = @FlatId", new[] {
                    new SqlParameter("@Status", flatStatus),
                    new SqlParameter("@FlatId", flatId)
                });

                // Ensure resident record exists
                int resCount = Convert.ToInt32(DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Residents WHERE UserId = @UserId AND FlatId = @FlatId", new[] {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@FlatId", flatId)
                }));

                if (resCount == 0 && status == "Active")
                {
                    DbHelper.ExecuteNonQuery(@"
                        INSERT INTO dbo.Residents (UserId, FlatId, TenantId, Relationship, Status)
                        VALUES (@UserId, @FlatId, @TenantId, 'Self', 'Active')", new[] {
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@FlatId", flatId),
                        new SqlParameter("@TenantId", tenantId)
                    });
                }

                UIHelper.ShowSuccess("Tenant Allocated Successfully!");
                ClearForm();
                LoadTenants();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error allocating tenant: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedTenantId <= 0)
            {
                UIHelper.ShowWarning("Please select a tenant record from the table to update.");
                return;
            }

            if (!ValidateInputs()) return;

            try
            {
                int userId = Convert.ToInt32(cmbUser.SelectedValue);
                int flatId = Convert.ToInt32(cmbFlat.SelectedValue);
                string status = cmbStatus.SelectedItem?.ToString() ?? "Active";

                string updateQuery = @"
                    UPDATE dbo.Tenants 
                    SET UserId = @UserId, FlatId = @FlatId, LeaseStartDate = @LeaseStartDate, 
                        LeaseEndDate = @LeaseEndDate, AgreedRent = @AgreedRent, 
                        SecurityDeposit = @SecurityDeposit, EmergencyContact = @EmergencyContact, Status = @Status
                    WHERE TenantId = @TenantId";

                var parameters = new[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@FlatId", flatId),
                    new SqlParameter("@LeaseStartDate", dtpLeaseStart.Value.Date),
                    new SqlParameter("@LeaseEndDate", dtpLeaseEnd.Value.Date),
                    new SqlParameter("@AgreedRent", decimal.Parse(txtAgreedRent.Text.Trim())),
                    new SqlParameter("@SecurityDeposit", decimal.Parse(txtSecurityDeposit.Text.Trim())),
                    new SqlParameter("@EmergencyContact", (object?)txtEmergencyContact.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Status", status),
                    new SqlParameter("@TenantId", selectedTenantId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);

                // Update Flat status
                string flatStatus = (status == "Active") ? "Occupied" : "Vacant";
                DbHelper.ExecuteNonQuery("UPDATE dbo.Flats SET Status = @Status WHERE FlatId = @FlatId", new[] {
                    new SqlParameter("@Status", flatStatus),
                    new SqlParameter("@FlatId", flatId)
                });

                UIHelper.ShowSuccess("Lease Contract Updated Successfully!");
                ClearForm();
                LoadTenants();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating tenant lease: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedTenantId <= 0)
            {
                UIHelper.ShowWarning("Please select a lease record to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to remove this tenant allocation?"))
            {
                return;
            }

            try
            {
                // Get flat ID first
                object? flatIdObj = DbHelper.ExecuteScalar("SELECT FlatId FROM dbo.Tenants WHERE TenantId = @TenantId",
                    new[] { new SqlParameter("@TenantId", selectedTenantId) });

                DbHelper.ExecuteNonQuery("DELETE FROM dbo.Tenants WHERE TenantId = @TenantId",
                    new[] { new SqlParameter("@TenantId", selectedTenantId) });

                if (flatIdObj != null && flatIdObj != DBNull.Value)
                {
                    DbHelper.ExecuteNonQuery("UPDATE dbo.Flats SET Status = 'Vacant' WHERE FlatId = @FlatId",
                        new[] { new SqlParameter("@FlatId", Convert.ToInt32(flatIdObj)) });
                }

                UIHelper.ShowSuccess("Tenant Allocation Deleted Successfully!");
                ClearForm();
                LoadTenants();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Cannot delete tenant allocation. Linked rent or payment records exist.\nDetails: {ex.Message}");
            }
        }

        private bool ValidateInputs()
        {
            if (cmbUser.SelectedValue == null || cmbFlat.SelectedValue == null)
            {
                UIHelper.ShowWarning("Please select both a Tenant User and an Allocated Flat.");
                return false;
            }

            if (!decimal.TryParse(txtAgreedRent.Text.Trim(), out decimal rent) || rent <= 0)
            {
                UIHelper.ShowWarning("Please enter a valid positive Agreed Rent amount.");
                return false;
            }

            if (!decimal.TryParse(txtSecurityDeposit.Text.Trim(), out decimal deposit) || deposit < 0)
            {
                UIHelper.ShowWarning("Please enter a valid Security Deposit amount.");
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            selectedTenantId = 0;
            txtAgreedRent.Clear();
            txtSecurityDeposit.Text = "0";
            txtEmergencyContact.Clear();
            dtpLeaseStart.Value = DateTime.Today;
            dtpLeaseEnd.Value = DateTime.Today.AddYears(1);
            cmbStatus.SelectedIndex = 0;
        }
    }
}
