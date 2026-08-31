using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class VisitorManagementForm : Form
    {
        private int selectedVisitorId = 0;
        private TextBox txtVisitorName = null!;
        private TextBox txtPhone = null!;
        private TextBox txtPurpose = null!;
        private ComboBox cmbFlat = null!;
        private DateTimePicker dtpCheckIn = null!;
        private ComboBox cmbStatus = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbStatusFilter = null!;
        private DataGridView dgvVisitors = null!;
        private Button btnAdd = null!;
        private Button btnCheckOut = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public VisitorManagementForm()
        {
            InitializeComponent();
            LoadFlatsDropdown();
            LoadVisitors();
        }

        private void InitializeComponent()
        {
            this.Text = "Visitor Log & Security Gate Management - NeighbourHub";
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
                Text = "🚪 Visitor Entry Log & Security Gate Access",
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
            inputPanel.Controls.Add(CreateLabel("Visitor Name *:", 16, top));
            txtVisitorName = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. Arman Khan" };
            inputPanel.Controls.Add(txtVisitorName);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Phone Number:", 16, top));
            txtPhone = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. 01711223344" };
            inputPanel.Controls.Add(txtPhone);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Visiting Flat *:", 16, top));
            cmbFlat = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbFlat);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Purpose of Visit *:", 16, top));
            txtPurpose = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. Family Visit, Courier, Delivery" };
            inputPanel.Controls.Add(txtPurpose);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Check-In Time *:", 16, top));
            dtpCheckIn = new DateTimePicker { Location = new Point(16, top + 22), Size = new Size(300, 28), Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd hh:mm tt" };
            inputPanel.Controls.Add(dtpCheckIn);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Visitor Status *:", 16, top));
            cmbStatus = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Inside", "Checked Out", "Expected" });
            cmbStatus.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbStatus);
            top += 58;

            // Buttons
            btnAdd = new Button { Text = "➕ Log Entry", Location = new Point(16, top), Size = new Size(140, 36) };
            UIHelper.StyleButton(btnAdd, ThemeColors.Primary, Color.White);
            btnAdd.Click += BtnAdd_Click;

            btnCheckOut = new Button { Text = "⏱️ Check Out", Location = new Point(165, top), Size = new Size(150, 36) };
            UIHelper.StyleButton(btnCheckOut, ThemeColors.Success, Color.White);
            btnCheckOut.Click += BtnCheckOut_Click;
            top += 42;

            btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(16, top), Size = new Size(140, 36) };
            UIHelper.StyleButton(btnDelete, ThemeColors.Danger, Color.White);
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "🔄 Clear", Location = new Point(165, top), Size = new Size(150, 36) };
            UIHelper.StyleButton(btnClear, ThemeColors.Secondary, Color.White);
            btnClear.Click += (s, e) => ClearForm();

            inputPanel.Controls.Add(btnAdd);
            inputPanel.Controls.Add(btnCheckOut);
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

            var lblSearch = new Label { Text = "Search Visitor:", Location = new Point(12, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(115, 11), Size = new Size(180, 28) };
            txtSearch.TextChanged += (s, e) => FilterVisitors();

            var lblFilterStatus = new Label { Text = "Status:", Location = new Point(310, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbStatusFilter = new ComboBox { Location = new Point(365, 11), Size = new Size(130, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Inside", "Checked Out", "Expected" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => FilterVisitors();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(510, 9), Size = new Size(75, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbStatusFilter.SelectedIndex = 0; LoadVisitors(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblFilterStatus);
            searchPanel.Controls.Add(cmbStatusFilter);
            searchPanel.Controls.Add(btnRefresh);

            dgvVisitors = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvVisitors);
            dgvVisitors.CellClick += DgvVisitors_CellClick;

            rightPanel.Controls.Add(dgvVisitors);
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

        private void LoadFlatsDropdown()
        {
            try
            {
                string query = @"
                    SELECT f.FlatId, 'Flat ' + f.FlatNumber + ' (' + b.BuildingName + ')' AS FlatDisplay
                    FROM dbo.Flats f
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId";

                var dt = DbHelper.ExecuteDataTable(query);
                cmbFlat.DataSource = dt;
                cmbFlat.DisplayMember = "FlatDisplay";
                cmbFlat.ValueMember = "FlatId";

                if (SessionManager.ResidentFlatId.HasValue)
                {
                    cmbFlat.SelectedValue = SessionManager.ResidentFlatId.Value;
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading flats: {ex.Message}");
            }
        }

        private void LoadVisitors()
        {
            try
            {
                string query = @"
                    SELECT v.VisitorId, v.VisitorName, v.Phone, f.FlatNumber, b.BuildingName,
                           v.Purpose, v.CheckInTime, v.CheckOutTime, v.Status,
                           u.FullName AS HostResident, v.FlatId, v.ResidentUserId
                    FROM dbo.Visitors v
                    INNER JOIN dbo.Flats f ON v.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Users u ON v.ResidentUserId = u.UserId ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                if (SessionManager.CurrentUser?.Role == "Resident")
                {
                    query += " WHERE v.ResidentUserId = @UserId ";
                    parameters.Add(new SqlParameter("@UserId", SessionManager.CurrentUser.UserId));
                }

                query += " ORDER BY v.VisitorId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvVisitors.DataSource = dt;

                if (dgvVisitors.Columns["VisitorId"] != null) dgvVisitors.Columns["VisitorId"].Width = 60;
                if (dgvVisitors.Columns["FlatId"] != null) dgvVisitors.Columns["FlatId"].Visible = false;
                if (dgvVisitors.Columns["ResidentUserId"] != null) dgvVisitors.Columns["ResidentUserId"].Visible = false;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load visitors: {ex.Message}");
            }
        }

        private void FilterVisitors()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string selectedStatus = cmbStatusFilter.SelectedItem?.ToString() ?? "All";

                string query = @"
                    SELECT v.VisitorId, v.VisitorName, v.Phone, f.FlatNumber, b.BuildingName,
                           v.Purpose, v.CheckInTime, v.CheckOutTime, v.Status,
                           u.FullName AS HostResident, v.FlatId, v.ResidentUserId
                    FROM dbo.Visitors v
                    INNER JOIN dbo.Flats f ON v.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Users u ON v.ResidentUserId = u.UserId
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (SessionManager.CurrentUser?.Role == "Resident")
                {
                    query += " AND v.ResidentUserId = @UserId ";
                    parameters.Add(new SqlParameter("@UserId", SessionManager.CurrentUser.UserId));
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (v.VisitorName LIKE @Keyword OR v.Phone LIKE @Keyword OR f.FlatNumber LIKE @Keyword OR v.Purpose LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                if (selectedStatus != "All")
                {
                    query += " AND v.Status = @Status ";
                    parameters.Add(new SqlParameter("@Status", selectedStatus));
                }

                query += " ORDER BY v.VisitorId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvVisitors.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvVisitors_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvVisitors.Rows.Count)
            {
                var row = dgvVisitors.Rows[e.RowIndex];
                selectedVisitorId = Convert.ToInt32(row.Cells["VisitorId"].Value);
                txtVisitorName.Text = row.Cells["VisitorName"].Value?.ToString() ?? string.Empty;
                txtPhone.Text = row.Cells["Phone"].Value?.ToString() ?? string.Empty;
                txtPurpose.Text = row.Cells["Purpose"].Value?.ToString() ?? string.Empty;

                if (row.Cells["CheckInTime"].Value != DBNull.Value)
                    dtpCheckIn.Value = Convert.ToDateTime(row.Cells["CheckInTime"].Value);

                if (row.Cells["FlatId"].Value != DBNull.Value)
                    cmbFlat.SelectedValue = Convert.ToInt32(row.Cells["FlatId"].Value);

                cmbStatus.SelectedItem = row.Cells["Status"].Value?.ToString() ?? "Inside";
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtVisitorName.Text) || cmbFlat.SelectedValue == null)
            {
                UIHelper.ShowWarning("Please fill in Visitor Name and Visiting Flat.");
                return;
            }

            try
            {
                int residentUserId = SessionManager.CurrentUser?.UserId ?? 1;

                string insertQuery = @"
                    INSERT INTO dbo.Visitors (FlatId, ResidentUserId, VisitorName, Phone, Purpose, CheckInTime, Status)
                    VALUES (@FlatId, @ResidentUserId, @VisitorName, @Phone, @Purpose, @CheckInTime, @Status)";

                var parameters = new[]
                {
                    new SqlParameter("@FlatId", Convert.ToInt32(cmbFlat.SelectedValue)),
                    new SqlParameter("@ResidentUserId", residentUserId),
                    new SqlParameter("@VisitorName", txtVisitorName.Text.Trim()),
                    new SqlParameter("@Phone", (object?)txtPhone.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Purpose", (object?)txtPurpose.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@CheckInTime", dtpCheckIn.Value),
                    new SqlParameter("@Status", cmbStatus.SelectedItem!.ToString())
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Visitor Entry Logged Successfully!");
                ClearForm();
                LoadVisitors();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error logging visitor: {ex.Message}");
            }
        }

        private void BtnCheckOut_Click(object? sender, EventArgs e)
        {
            if (selectedVisitorId <= 0)
            {
                UIHelper.ShowWarning("Please select a visitor from the table to check out.");
                return;
            }

            try
            {
                string updateQuery = @"
                    UPDATE dbo.Visitors 
                    SET CheckOutTime = GETDATE(), Status = 'Checked Out'
                    WHERE VisitorId = @VisitorId";

                DbHelper.ExecuteNonQuery(updateQuery, new[] { new SqlParameter("@VisitorId", selectedVisitorId) });
                UIHelper.ShowSuccess("Visitor Checked Out Successfully!");
                ClearForm();
                LoadVisitors();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error checking out visitor: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedVisitorId <= 0)
            {
                UIHelper.ShowWarning("Please select a visitor log to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this visitor record?"))
            {
                return;
            }

            try
            {
                DbHelper.ExecuteNonQuery("DELETE FROM dbo.Visitors WHERE VisitorId = @VisitorId",
                    new[] { new SqlParameter("@VisitorId", selectedVisitorId) });

                UIHelper.ShowSuccess("Visitor Record Deleted Successfully!");
                ClearForm();
                LoadVisitors();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error deleting visitor record: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            selectedVisitorId = 0;
            txtVisitorName.Clear();
            txtPhone.Clear();
            txtPurpose.Clear();
            dtpCheckIn.Value = DateTime.Now;
            cmbStatus.SelectedIndex = 0;
        }
    }
}
