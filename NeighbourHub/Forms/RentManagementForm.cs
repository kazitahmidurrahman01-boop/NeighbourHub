using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class RentManagementForm : Form
    {
        private int selectedRentId = 0;
        private ComboBox cmbTenant = null!;
        private ComboBox cmbMonth = null!;
        private NumericUpDown numYear = null!;
        private TextBox txtRentAmount = null!;
        private TextBox txtUtilityCharges = null!;
        private DateTimePicker dtpDueDate = null!;
        private ComboBox cmbStatus = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbStatusFilter = null!;
        private DataGridView dgvRent = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;
        private Button btnBatchGenerate = null!;
        private Label lblRentSummary = null!;

        public RentManagementForm()
        {
            InitializeComponent();
            LoadTenantsDropdown();
            LoadRentRecords();
        }

        private void InitializeComponent()
        {
            this.Text = "Rent Management & Billing - NeighbourHub";
            this.Size = new Size(1080, 700);
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
                Text = "💰 Rent Billing & Invoicing Management",
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
            inputPanel.Controls.Add(CreateLabel("Tenant & Flat *:", 16, top));
            cmbTenant = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTenant.SelectedIndexChanged += CmbTenant_SelectedIndexChanged;
            inputPanel.Controls.Add(cmbTenant);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Billing Month & Year *:", 16, top));
            cmbMonth = new ComboBox { Location = new Point(16, top + 22), Size = new Size(160, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMonth.Items.AddRange(new object[] {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            });
            cmbMonth.SelectedItem = DateTime.Now.ToString("MMMM");

            numYear = new NumericUpDown { Location = new Point(186, top + 22), Size = new Size(130, 28), Minimum = 2020, Maximum = 2050, Value = DateTime.Now.Year };
            inputPanel.Controls.Add(cmbMonth);
            inputPanel.Controls.Add(numYear);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Rent Amount (৳) *:", 16, top));
            txtRentAmount = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtRentAmount);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Utility Charges / Service (৳):", 16, top));
            txtUtilityCharges = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), Text = "2000" };
            inputPanel.Controls.Add(txtUtilityCharges);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Payment Due Date *:", 16, top));
            dtpDueDate = new DateTimePicker { Location = new Point(16, top + 22), Size = new Size(300, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(10) };
            inputPanel.Controls.Add(dtpDueDate);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Payment Status *:", 16, top));
            cmbStatus = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Due", "Paid", "Partially Paid" });
            cmbStatus.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbStatus);
            top += 58;

            // Buttons
            btnAdd = new Button { Text = "➕ Create Bill", Location = new Point(16, top), Size = new Size(140, 36) };
            UIHelper.StyleButton(btnAdd, ThemeColors.Primary, Color.White);
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button { Text = "💾 Update", Location = new Point(165, top), Size = new Size(150, 36) };
            UIHelper.StyleButton(btnUpdate, ThemeColors.Success, Color.White);
            btnUpdate.Click += BtnUpdate_Click;
            top += 42;

            btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(16, top), Size = new Size(140, 36) };
            UIHelper.StyleButton(btnDelete, ThemeColors.Danger, Color.White);
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "🔄 Clear", Location = new Point(165, top), Size = new Size(150, 36) };
            UIHelper.StyleButton(btnClear, ThemeColors.Secondary, Color.White);
            btnClear.Click += (s, e) => ClearForm();
            top += 46;

            btnBatchGenerate = new Button { Text = "⚡ Auto-Generate All Active Rent", Location = new Point(16, top), Size = new Size(300, 36) };
            UIHelper.StyleButton(btnBatchGenerate, ThemeColors.Purple, Color.White);
            btnBatchGenerate.Click += BtnBatchGenerate_Click;

            inputPanel.Controls.Add(btnAdd);
            inputPanel.Controls.Add(btnUpdate);
            inputPanel.Controls.Add(btnDelete);
            inputPanel.Controls.Add(btnClear);
            inputPanel.Controls.Add(btnBatchGenerate);

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
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblSearch = new Label { Text = "Search:", Location = new Point(12, 12), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(65, 9), Size = new Size(160, 28) };
            txtSearch.TextChanged += (s, e) => FilterRentRecords();

            var lblFilterStatus = new Label { Text = "Status:", Location = new Point(235, 12), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbStatusFilter = new ComboBox { Location = new Point(285, 9), Size = new Size(120, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Due", "Paid", "Partially Paid" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => FilterRentRecords();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(415, 7), Size = new Size(75, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbStatusFilter.SelectedIndex = 0; LoadRentRecords(); };

            lblRentSummary = new Label
            {
                Text = "Expected: ৳0 | Collected: ৳0 | Due: ৳0",
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.PrimaryDark,
                Location = new Point(12, 42),
                AutoSize = true
            };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblFilterStatus);
            searchPanel.Controls.Add(cmbStatusFilter);
            searchPanel.Controls.Add(btnRefresh);
            searchPanel.Controls.Add(lblRentSummary);

            dgvRent = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvRent);
            dgvRent.CellClick += DgvRent_CellClick;

            rightPanel.Controls.Add(dgvRent);
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

        private void LoadTenantsDropdown()
        {
            try
            {
                string query = @"
                    SELECT t.TenantId, u.FullName + ' (Flat ' + f.FlatNumber + ' - ' + b.BuildingName + ')' AS TenantDisplay,
                           t.AgreedRent, t.FlatId
                    FROM dbo.Tenants t
                    INNER JOIN dbo.Users u ON t.UserId = u.UserId
                    INNER JOIN dbo.Flats f ON t.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId
                    WHERE t.Status = 'Active'";

                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    query += $" AND p.OwnerUserId = {SessionManager.CurrentUser.UserId}";
                }
                else if (SessionManager.CurrentUser?.Role == "Resident")
                {
                    query += $" AND t.UserId = {SessionManager.CurrentUser.UserId}";
                }

                var dt = DbHelper.ExecuteDataTable(query);
                cmbTenant.DataSource = dt;
                cmbTenant.DisplayMember = "TenantDisplay";
                cmbTenant.ValueMember = "TenantId";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading tenants: {ex.Message}");
            }
        }

        private void CmbTenant_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbTenant.SelectedItem is DataRowView drv)
            {
                if (drv.Row.Table.Columns.Contains("AgreedRent"))
                {
                    txtRentAmount.Text = drv["AgreedRent"].ToString();
                }
            }
        }

        private void LoadRentRecords()
        {
            try
            {
                string query = @"
                    SELECT r.RentId, u.FullName AS TenantName, f.FlatNumber, b.BuildingName,
                           r.Month, r.Year, r.RentAmount, r.UtilityCharges,
                           (r.RentAmount + r.UtilityCharges) AS TotalAmount,
                           r.DueDate, r.Status, r.TenantId, r.FlatId
                    FROM dbo.Rent r
                    INNER JOIN dbo.Tenants t ON r.TenantId = t.TenantId
                    INNER JOIN dbo.Users u ON t.UserId = u.UserId
                    INNER JOIN dbo.Flats f ON r.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    query += " WHERE p.OwnerUserId = @OwnerId ";
                    parameters.Add(new SqlParameter("@OwnerId", SessionManager.CurrentUser.UserId));
                }
                else if (SessionManager.CurrentUser?.Role == "Resident")
                {
                    query += " WHERE t.UserId = @UserId ";
                    parameters.Add(new SqlParameter("@UserId", SessionManager.CurrentUser.UserId));
                }

                query += " ORDER BY r.RentId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvRent.DataSource = dt;

                if (dgvRent.Columns["RentId"] != null) dgvRent.Columns["RentId"].Width = 60;
                if (dgvRent.Columns["TenantId"] != null) dgvRent.Columns["TenantId"].Visible = false;
                if (dgvRent.Columns["FlatId"] != null) dgvRent.Columns["FlatId"].Visible = false;

                UpdateSummary(dt);
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load rent records: {ex.Message}");
            }
        }

        private void FilterRentRecords()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string selectedStatus = cmbStatusFilter.SelectedItem?.ToString() ?? "All";

                string query = @"
                    SELECT r.RentId, u.FullName AS TenantName, f.FlatNumber, b.BuildingName,
                           r.Month, r.Year, r.RentAmount, r.UtilityCharges,
                           (r.RentAmount + r.UtilityCharges) AS TotalAmount,
                           r.DueDate, r.Status, r.TenantId, r.FlatId
                    FROM dbo.Rent r
                    INNER JOIN dbo.Tenants t ON r.TenantId = t.TenantId
                    INNER JOIN dbo.Users u ON t.UserId = u.UserId
                    INNER JOIN dbo.Flats f ON r.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    query += " AND p.OwnerUserId = @OwnerId ";
                    parameters.Add(new SqlParameter("@OwnerId", SessionManager.CurrentUser.UserId));
                }
                else if (SessionManager.CurrentUser?.Role == "Resident")
                {
                    query += " AND t.UserId = @UserId ";
                    parameters.Add(new SqlParameter("@UserId", SessionManager.CurrentUser.UserId));
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (u.FullName LIKE @Keyword OR f.FlatNumber LIKE @Keyword OR b.BuildingName LIKE @Keyword OR r.Month LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                if (selectedStatus != "All")
                {
                    query += " AND r.Status = @Status ";
                    parameters.Add(new SqlParameter("@Status", selectedStatus));
                }

                query += " ORDER BY r.RentId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvRent.DataSource = dt;
                UpdateSummary(dt);
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void UpdateSummary(DataTable dt)
        {
            decimal totalExpected = 0;
            decimal totalCollected = 0;
            decimal totalDue = 0;

            foreach (DataRow row in dt.Rows)
            {
                decimal total = Convert.ToDecimal(row["TotalAmount"]);
                string st = row["Status"].ToString() ?? "";
                totalExpected += total;
                if (st == "Paid") totalCollected += total;
                else totalDue += total;
            }

            lblRentSummary.Text = $"📊 Total Expected: ৳{totalExpected:N0}  |  Collected: ৳{totalCollected:N0}  |  Due: ৳{totalDue:N0}";
        }

        private void DgvRent_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvRent.Rows.Count)
            {
                var row = dgvRent.Rows[e.RowIndex];
                selectedRentId = Convert.ToInt32(row.Cells["RentId"].Value);
                txtRentAmount.Text = row.Cells["RentAmount"].Value?.ToString() ?? "0";
                txtUtilityCharges.Text = row.Cells["UtilityCharges"].Value?.ToString() ?? "0";

                string m = row.Cells["Month"].Value?.ToString() ?? "August";
                cmbMonth.SelectedItem = m;
                numYear.Value = Convert.ToDecimal(row.Cells["Year"].Value ?? DateTime.Now.Year);

                if (row.Cells["DueDate"].Value != DBNull.Value)
                    dtpDueDate.Value = Convert.ToDateTime(row.Cells["DueDate"].Value);

                if (row.Cells["TenantId"].Value != DBNull.Value)
                    cmbTenant.SelectedValue = Convert.ToInt32(row.Cells["TenantId"].Value);

                cmbStatus.SelectedItem = row.Cells["Status"].Value?.ToString() ?? "Due";
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                int tenantId = Convert.ToInt32(cmbTenant.SelectedValue);

                // Get flatId for tenant
                object? flatIdObj = DbHelper.ExecuteScalar("SELECT FlatId FROM dbo.Tenants WHERE TenantId = @TenantId",
                    new[] { new SqlParameter("@TenantId", tenantId) });

                if (flatIdObj == null)
                {
                    UIHelper.ShowWarning("Selected tenant does not have an active flat.");
                    return;
                }

                int flatId = Convert.ToInt32(flatIdObj);
                string month = cmbMonth.SelectedItem?.ToString() ?? "August";
                int year = (int)numYear.Value;

                // Check duplicate bill for same month/year
                string checkQuery = "SELECT COUNT(*) FROM dbo.Rent WHERE FlatId = @FlatId AND Month = @Month AND Year = @Year";
                int count = Convert.ToInt32(DbHelper.ExecuteScalar(checkQuery, new[] {
                    new SqlParameter("@FlatId", flatId),
                    new SqlParameter("@Month", month),
                    new SqlParameter("@Year", year)
                }));

                if (count > 0)
                {
                    UIHelper.ShowWarning($"A rent bill for {month} {year} already exists for this flat.");
                    return;
                }

                string insertQuery = @"
                    INSERT INTO dbo.Rent (FlatId, TenantId, Month, Year, RentAmount, UtilityCharges, DueDate, Status)
                    VALUES (@FlatId, @TenantId, @Month, @Year, @RentAmount, @UtilityCharges, @DueDate, @Status)";

                var parameters = new[]
                {
                    new SqlParameter("@FlatId", flatId),
                    new SqlParameter("@TenantId", tenantId),
                    new SqlParameter("@Month", month),
                    new SqlParameter("@Year", year),
                    new SqlParameter("@RentAmount", decimal.Parse(txtRentAmount.Text.Trim())),
                    new SqlParameter("@UtilityCharges", decimal.Parse(txtUtilityCharges.Text.Trim())),
                    new SqlParameter("@DueDate", dtpDueDate.Value.Date),
                    new SqlParameter("@Status", cmbStatus.SelectedItem!.ToString())
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Rent Bill Created Successfully!");
                ClearForm();
                LoadRentRecords();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error creating rent bill: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedRentId <= 0)
            {
                UIHelper.ShowWarning("Please select a rent record from the table to update.");
                return;
            }

            if (!ValidateInputs()) return;

            try
            {
                string updateQuery = @"
                    UPDATE dbo.Rent 
                    SET Month = @Month, Year = @Year, RentAmount = @RentAmount, 
                        UtilityCharges = @UtilityCharges, DueDate = @DueDate, Status = @Status
                    WHERE RentId = @RentId";

                var parameters = new[]
                {
                    new SqlParameter("@Month", cmbMonth.SelectedItem!.ToString()),
                    new SqlParameter("@Year", (int)numYear.Value),
                    new SqlParameter("@RentAmount", decimal.Parse(txtRentAmount.Text.Trim())),
                    new SqlParameter("@UtilityCharges", decimal.Parse(txtUtilityCharges.Text.Trim())),
                    new SqlParameter("@DueDate", dtpDueDate.Value.Date),
                    new SqlParameter("@Status", cmbStatus.SelectedItem!.ToString()),
                    new SqlParameter("@RentId", selectedRentId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Rent Bill Updated Successfully!");
                ClearForm();
                LoadRentRecords();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating rent record: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedRentId <= 0)
            {
                UIHelper.ShowWarning("Please select a rent bill to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this rent bill?"))
            {
                return;
            }

            try
            {
                DbHelper.ExecuteNonQuery("DELETE FROM dbo.Rent WHERE RentId = @RentId",
                    new[] { new SqlParameter("@RentId", selectedRentId) });

                UIHelper.ShowSuccess("Rent Bill Deleted Successfully!");
                ClearForm();
                LoadRentRecords();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Cannot delete rent record. Linked payment records exist.\nDetails: {ex.Message}");
            }
        }

        private void BtnBatchGenerate_Click(object? sender, EventArgs e)
        {
            string currentMonth = cmbMonth.SelectedItem?.ToString() ?? DateTime.Now.ToString("MMMM");
            int currentYear = (int)numYear.Value;

            if (!UIHelper.Confirm($"Generate rent bills for ALL active tenants for {currentMonth} {currentYear}?"))
            {
                return;
            }

            try
            {
                string tenantQuery = @"
                    SELECT t.TenantId, t.FlatId, t.AgreedRent
                    FROM dbo.Tenants t
                    WHERE t.Status = 'Active'";

                var dtActiveTenants = DbHelper.ExecuteDataTable(tenantQuery);
                int generatedCount = 0;

                foreach (DataRow row in dtActiveTenants.Rows)
                {
                    int tenantId = Convert.ToInt32(row["TenantId"]);
                    int flatId = Convert.ToInt32(row["FlatId"]);
                    decimal agreedRent = Convert.ToDecimal(row["AgreedRent"]);

                    // Check if already generated
                    string check = "SELECT COUNT(*) FROM dbo.Rent WHERE FlatId = @FlatId AND Month = @Month AND Year = @Year";
                    int exists = Convert.ToInt32(DbHelper.ExecuteScalar(check, new[] {
                        new SqlParameter("@FlatId", flatId),
                        new SqlParameter("@Month", currentMonth),
                        new SqlParameter("@Year", currentYear)
                    }));

                    if (exists == 0)
                    {
                        string insert = @"
                            INSERT INTO dbo.Rent (FlatId, TenantId, Month, Year, RentAmount, UtilityCharges, DueDate, Status)
                            VALUES (@FlatId, @TenantId, @Month, @Year, @RentAmount, 2000, @DueDate, 'Due')";

                        DbHelper.ExecuteNonQuery(insert, new[] {
                            new SqlParameter("@FlatId", flatId),
                            new SqlParameter("@TenantId", tenantId),
                            new SqlParameter("@Month", currentMonth),
                            new SqlParameter("@Year", currentYear),
                            new SqlParameter("@RentAmount", agreedRent),
                            new SqlParameter("@DueDate", DateTime.Today.AddDays(10))
                        });
                        generatedCount++;
                    }
                }

                UIHelper.ShowSuccess($"Batch generation complete!\nCreated {generatedCount} new rent bill(s) for {currentMonth} {currentYear}.");
                LoadRentRecords();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error in batch generation: {ex.Message}");
            }
        }

        private bool ValidateInputs()
        {
            if (cmbTenant.SelectedValue == null)
            {
                UIHelper.ShowWarning("Please select an active Tenant.");
                return false;
            }

            if (!decimal.TryParse(txtRentAmount.Text.Trim(), out decimal rent) || rent <= 0)
            {
                UIHelper.ShowWarning("Please enter a valid positive Rent Amount.");
                return false;
            }

            if (!decimal.TryParse(txtUtilityCharges.Text.Trim(), out decimal util) || util < 0)
            {
                UIHelper.ShowWarning("Please enter a valid non-negative Utility Charges amount.");
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            selectedRentId = 0;
            txtRentAmount.Clear();
            txtUtilityCharges.Text = "2000";
            dtpDueDate.Value = DateTime.Today.AddDays(10);
            cmbStatus.SelectedIndex = 0;
        }
    }
}
