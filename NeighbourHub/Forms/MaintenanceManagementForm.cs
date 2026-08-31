using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class MaintenanceManagementForm : Form
    {
        private int selectedMaintenanceId = 0;
        private TextBox txtTitle = null!;
        private ComboBox cmbBuilding = null!;
        private TextBox txtCost = null!;
        private TextBox txtVendorName = null!;
        private DateTimePicker dtpScheduled = null!;
        private DateTimePicker dtpCompletion = null!;
        private ComboBox cmbStatus = null!;
        private TextBox txtDescription = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbStatusFilter = null!;
        private DataGridView dgvMaintenance = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;
        private Label lblTotalExpense = null!;

        public MaintenanceManagementForm()
        {
            InitializeComponent();
            LoadBuildingsDropdown();
            LoadMaintenance();
        }

        private void InitializeComponent()
        {
            this.Text = "Maintenance & Repairs Management - NeighbourHub";
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
                Text = "🔧 Building Maintenance, Servicing & Vendor Management",
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
            inputPanel.Controls.Add(CreateLabel("Maintenance Title *:", 16, top));
            txtTitle = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. Elevator Routine Servicing" };
            inputPanel.Controls.Add(txtTitle);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Building *:", 16, top));
            cmbBuilding = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbBuilding);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Estimated / Actual Cost (৳) *:", 16, top));
            txtCost = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), Text = "0" };
            inputPanel.Controls.Add(txtCost);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Vendor / Contractor Name:", 16, top));
            txtVendorName = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. Otis Bangladesh Ltd" };
            inputPanel.Controls.Add(txtVendorName);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Scheduled Date *:", 16, top));
            dtpScheduled = new DateTimePicker { Location = new Point(16, top + 22), Size = new Size(300, 28), Format = DateTimePickerFormat.Short };
            inputPanel.Controls.Add(dtpScheduled);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Completion Date:", 16, top));
            dtpCompletion = new DateTimePicker { Location = new Point(16, top + 22), Size = new Size(300, 28), Format = DateTimePickerFormat.Short };
            inputPanel.Controls.Add(dtpCompletion);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Status *:", 16, top));
            cmbStatus = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Scheduled", "In Progress", "Completed", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbStatus);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Description / Work Details:", 16, top));
            txtDescription = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 60), Multiline = true, ScrollBars = ScrollBars.Vertical };
            inputPanel.Controls.Add(txtDescription);
            top += 90;

            // Buttons
            btnAdd = new Button { Text = "➕ Add Work", Location = new Point(16, top), Size = new Size(140, 36) };
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
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblSearch = new Label { Text = "Search:", Location = new Point(12, 12), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(65, 9), Size = new Size(180, 28) };
            txtSearch.TextChanged += (s, e) => FilterMaintenance();

            var lblFilterStatus = new Label { Text = "Status:", Location = new Point(255, 12), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbStatusFilter = new ComboBox { Location = new Point(310, 9), Size = new Size(130, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Scheduled", "In Progress", "Completed", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => FilterMaintenance();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(450, 7), Size = new Size(75, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbStatusFilter.SelectedIndex = 0; LoadMaintenance(); };

            lblTotalExpense = new Label
            {
                Text = "Total Maintenance Expense: ৳0",
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.Purple,
                Location = new Point(12, 42),
                AutoSize = true
            };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblFilterStatus);
            searchPanel.Controls.Add(cmbStatusFilter);
            searchPanel.Controls.Add(btnRefresh);
            searchPanel.Controls.Add(lblTotalExpense);

            dgvMaintenance = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvMaintenance);
            dgvMaintenance.CellClick += DgvMaintenance_CellClick;

            rightPanel.Controls.Add(dgvMaintenance);
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

        private void LoadBuildingsDropdown()
        {
            try
            {
                var dtBuildings = DbHelper.ExecuteDataTable("SELECT BuildingId, BuildingName FROM dbo.Buildings");
                cmbBuilding.DataSource = dtBuildings;
                cmbBuilding.DisplayMember = "BuildingName";
                cmbBuilding.ValueMember = "BuildingId";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading buildings: {ex.Message}");
            }
        }

        private void LoadMaintenance()
        {
            try
            {
                string query = @"
                    SELECT m.MaintenanceId, m.Title, b.BuildingName, m.Cost, m.VendorName,
                           m.ScheduledDate, m.CompletionDate, m.Status, m.Description, m.BuildingId
                    FROM dbo.Maintenance m
                    INNER JOIN dbo.Buildings b ON m.BuildingId = b.BuildingId
                    ORDER BY m.MaintenanceId DESC";

                var dt = DbHelper.ExecuteDataTable(query);
                dgvMaintenance.DataSource = dt;

                if (dgvMaintenance.Columns["MaintenanceId"] != null) dgvMaintenance.Columns["MaintenanceId"].Width = 60;
                if (dgvMaintenance.Columns["BuildingId"] != null) dgvMaintenance.Columns["BuildingId"].Visible = false;
                if (dgvMaintenance.Columns["Description"] != null) dgvMaintenance.Columns["Description"].Visible = false;

                decimal sum = 0;
                foreach (DataRow row in dt.Rows)
                {
                    sum += Convert.ToDecimal(row["Cost"]);
                }
                lblTotalExpense.Text = $"🛠️ Total Maintenance Expense: ৳{sum:N0}";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load maintenance records: {ex.Message}");
            }
        }

        private void FilterMaintenance()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string selectedStatus = cmbStatusFilter.SelectedItem?.ToString() ?? "All";

                string query = @"
                    SELECT m.MaintenanceId, m.Title, b.BuildingName, m.Cost, m.VendorName,
                           m.ScheduledDate, m.CompletionDate, m.Status, m.Description, m.BuildingId
                    FROM dbo.Maintenance m
                    INNER JOIN dbo.Buildings b ON m.BuildingId = b.BuildingId
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (m.Title LIKE @Keyword OR m.VendorName LIKE @Keyword OR b.BuildingName LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                if (selectedStatus != "All")
                {
                    query += " AND m.Status = @Status ";
                    parameters.Add(new SqlParameter("@Status", selectedStatus));
                }

                query += " ORDER BY m.MaintenanceId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvMaintenance.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvMaintenance_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvMaintenance.Rows.Count)
            {
                var row = dgvMaintenance.Rows[e.RowIndex];
                selectedMaintenanceId = Convert.ToInt32(row.Cells["MaintenanceId"].Value);
                txtTitle.Text = row.Cells["Title"].Value?.ToString() ?? string.Empty;
                txtCost.Text = row.Cells["Cost"].Value?.ToString() ?? "0";
                txtVendorName.Text = row.Cells["VendorName"].Value?.ToString() ?? string.Empty;
                txtDescription.Text = row.Cells["Description"].Value?.ToString() ?? string.Empty;

                if (row.Cells["ScheduledDate"].Value != DBNull.Value)
                    dtpScheduled.Value = Convert.ToDateTime(row.Cells["ScheduledDate"].Value);

                if (row.Cells["CompletionDate"].Value != DBNull.Value)
                    dtpCompletion.Value = Convert.ToDateTime(row.Cells["CompletionDate"].Value);

                if (row.Cells["BuildingId"].Value != DBNull.Value)
                    cmbBuilding.SelectedValue = Convert.ToInt32(row.Cells["BuildingId"].Value);

                cmbStatus.SelectedItem = row.Cells["Status"].Value?.ToString() ?? "Scheduled";
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || cmbBuilding.SelectedValue == null)
            {
                UIHelper.ShowWarning("Please fill in Title and select a Building.");
                return;
            }

            if (!decimal.TryParse(txtCost.Text.Trim(), out decimal cost) || cost < 0)
            {
                UIHelper.ShowWarning("Please enter a valid non-negative cost amount.");
                return;
            }

            try
            {
                string status = cmbStatus.SelectedItem?.ToString() ?? "Scheduled";
                object completionDate = (status == "Completed") ? (object)dtpCompletion.Value.Date : DBNull.Value;

                string insertQuery = @"
                    INSERT INTO dbo.Maintenance (BuildingId, Title, Description, Cost, VendorName, ScheduledDate, CompletionDate, Status)
                    VALUES (@BuildingId, @Title, @Description, @Cost, @VendorName, @ScheduledDate, @CompletionDate, @Status)";

                var parameters = new[]
                {
                    new SqlParameter("@BuildingId", Convert.ToInt32(cmbBuilding.SelectedValue)),
                    new SqlParameter("@Title", txtTitle.Text.Trim()),
                    new SqlParameter("@Description", (object?)txtDescription.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Cost", cost),
                    new SqlParameter("@VendorName", (object?)txtVendorName.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@ScheduledDate", dtpScheduled.Value.Date),
                    new SqlParameter("@CompletionDate", completionDate),
                    new SqlParameter("@Status", status)
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Maintenance Work Scheduled Successfully!");
                ClearForm();
                LoadMaintenance();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error scheduling maintenance: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedMaintenanceId <= 0)
            {
                UIHelper.ShowWarning("Please select a maintenance record from the table to update.");
                return;
            }

            if (!decimal.TryParse(txtCost.Text.Trim(), out decimal cost) || cost < 0)
            {
                UIHelper.ShowWarning("Please enter a valid non-negative cost amount.");
                return;
            }

            try
            {
                string status = cmbStatus.SelectedItem?.ToString() ?? "Scheduled";
                object completionDate = (status == "Completed") ? (object)dtpCompletion.Value.Date : DBNull.Value;

                string updateQuery = @"
                    UPDATE dbo.Maintenance 
                    SET BuildingId = @BuildingId, Title = @Title, Description = @Description, 
                        Cost = @Cost, VendorName = @VendorName, ScheduledDate = @ScheduledDate, 
                        CompletionDate = @CompletionDate, Status = @Status
                    WHERE MaintenanceId = @MaintenanceId";

                var parameters = new[]
                {
                    new SqlParameter("@BuildingId", Convert.ToInt32(cmbBuilding.SelectedValue)),
                    new SqlParameter("@Title", txtTitle.Text.Trim()),
                    new SqlParameter("@Description", (object?)txtDescription.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Cost", cost),
                    new SqlParameter("@VendorName", (object?)txtVendorName.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@ScheduledDate", dtpScheduled.Value.Date),
                    new SqlParameter("@CompletionDate", completionDate),
                    new SqlParameter("@Status", status),
                    new SqlParameter("@MaintenanceId", selectedMaintenanceId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Maintenance Work Updated Successfully!");
                ClearForm();
                LoadMaintenance();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating maintenance record: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedMaintenanceId <= 0)
            {
                UIHelper.ShowWarning("Please select a maintenance record to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this maintenance record?"))
            {
                return;
            }

            try
            {
                DbHelper.ExecuteNonQuery("DELETE FROM dbo.Maintenance WHERE MaintenanceId = @MaintenanceId",
                    new[] { new SqlParameter("@MaintenanceId", selectedMaintenanceId) });

                UIHelper.ShowSuccess("Maintenance Record Deleted Successfully!");
                ClearForm();
                LoadMaintenance();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error deleting maintenance record: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            selectedMaintenanceId = 0;
            txtTitle.Clear();
            txtDescription.Clear();
            txtCost.Text = "0";
            txtVendorName.Clear();
            dtpScheduled.Value = DateTime.Today;
            dtpCompletion.Value = DateTime.Today;
            cmbStatus.SelectedIndex = 0;
        }
    }
}
