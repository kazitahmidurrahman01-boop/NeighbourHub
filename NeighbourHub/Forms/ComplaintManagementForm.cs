using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class ComplaintManagementForm : Form
    {
        private int selectedComplaintId = 0;
        private TextBox txtTitle = null!;
        private ComboBox cmbCategory = null!;
        private ComboBox cmbPriority = null!;
        private ComboBox cmbBuilding = null!;
        private TextBox txtDescription = null!;
        private ComboBox cmbStatus = null!;
        private ComboBox cmbAssignedManager = null!;
        private TextBox txtResolutionNotes = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbStatusFilter = null!;
        private ComboBox cmbCategoryFilter = null!;
        private DataGridView dgvComplaints = null!;
        private Button btnAdd = null!;
        private Button btnUpdateStatus = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public ComplaintManagementForm()
        {
            InitializeComponent();
            LoadDropdowns();
            LoadComplaints();
        }

        private void InitializeComponent()
        {
            this.Text = "Complaints & Helpdesk - NeighbourHub";
            this.Size = new Size(1100, 720);
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
                Text = "🛠️ Resident Complaints & Maintenance Helpdesk",
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
                Width = 360,
                BackColor = Color.White,
                Padding = new Padding(16),
                AutoScroll = true
            };

            int top = 16;
            inputPanel.Controls.Add(CreateLabel("Complaint Title *:", 16, top));
            txtTitle = new TextBox { Location = new Point(16, top + 22), Size = new Size(310, 28), PlaceholderText = "e.g. 5th floor light is damaged." };
            inputPanel.Controls.Add(txtTitle);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Category *:", 16, top));
            cmbCategory = new ComboBox { Location = new Point(16, top + 22), Size = new Size(310, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.AddRange(new object[] { "Plumbing", "Electrical", "Elevator", "Security", "Cleanliness", "Noise", "Other" });
            cmbCategory.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbCategory);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Priority Level *:", 16, top));
            cmbPriority = new ComboBox { Location = new Point(16, top + 22), Size = new Size(310, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPriority.Items.AddRange(new object[] { "Low", "Medium", "High", "Urgent" });
            cmbPriority.SelectedIndex = 1;
            inputPanel.Controls.Add(cmbPriority);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Building *:", 16, top));
            cmbBuilding = new ComboBox { Location = new Point(16, top + 22), Size = new Size(310, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbBuilding);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Detailed Description *:", 16, top));
            txtDescription = new TextBox { Location = new Point(16, top + 22), Size = new Size(310, 60), Multiline = true, ScrollBars = ScrollBars.Vertical };
            inputPanel.Controls.Add(txtDescription);
            top += 88;

            // Manager Controls (Status & Resolution)
            inputPanel.Controls.Add(CreateLabel("Complaint Status (Manager/Admin):", 16, top));
            cmbStatus = new ComboBox { Location = new Point(16, top + 22), Size = new Size(310, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Pending", "In Progress", "Solved", "Rejected" });
            cmbStatus.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbStatus);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Assign Manager:", 16, top));
            cmbAssignedManager = new ComboBox { Location = new Point(16, top + 22), Size = new Size(310, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbAssignedManager);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Resolution Notes / Action Taken:", 16, top));
            txtResolutionNotes = new TextBox { Location = new Point(16, top + 22), Size = new Size(310, 50), Multiline = true, ScrollBars = ScrollBars.Vertical, PlaceholderText = "e.g. Electrician replaced bulb." };
            inputPanel.Controls.Add(txtResolutionNotes);
            top += 78;

            // Buttons
            btnAdd = new Button { Text = "📩 Submit", Location = new Point(16, top), Size = new Size(145, 36) };
            UIHelper.StyleButton(btnAdd, ThemeColors.Primary, Color.White);
            btnAdd.Click += BtnAdd_Click;

            btnUpdateStatus = new Button { Text = "🔄 Update Status", Location = new Point(170, top), Size = new Size(155, 36) };
            UIHelper.StyleButton(btnUpdateStatus, ThemeColors.Success, Color.White);
            btnUpdateStatus.Click += BtnUpdateStatus_Click;
            top += 42;

            btnDelete = new Button { Text = "🗑️ Delete", Location = new Point(16, top), Size = new Size(145, 36) };
            UIHelper.StyleButton(btnDelete, ThemeColors.Danger, Color.White);
            btnDelete.Click += BtnDelete_Click;

            btnClear = new Button { Text = "🔄 Clear", Location = new Point(170, top), Size = new Size(155, 36) };
            UIHelper.StyleButton(btnClear, ThemeColors.Secondary, Color.White);
            btnClear.Click += (s, e) => ClearForm();

            inputPanel.Controls.Add(btnAdd);
            inputPanel.Controls.Add(btnUpdateStatus);
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

            var lblSearch = new Label { Text = "Search:", Location = new Point(12, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(65, 11), Size = new Size(160, 28) };
            txtSearch.TextChanged += (s, e) => FilterComplaints();

            var lblFilterStatus = new Label { Text = "Status:", Location = new Point(235, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbStatusFilter = new ComboBox { Location = new Point(285, 11), Size = new Size(110, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Pending", "In Progress", "Solved", "Rejected" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => FilterComplaints();

            var lblFilterCat = new Label { Text = "Category:", Location = new Point(405, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbCategoryFilter = new ComboBox { Location = new Point(475, 11), Size = new Size(120, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategoryFilter.Items.AddRange(new object[] { "All", "Plumbing", "Electrical", "Elevator", "Security", "Cleanliness", "Noise", "Other" });
            cmbCategoryFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndexChanged += (s, e) => FilterComplaints();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(605, 9), Size = new Size(75, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbStatusFilter.SelectedIndex = 0; cmbCategoryFilter.SelectedIndex = 0; LoadComplaints(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblFilterStatus);
            searchPanel.Controls.Add(cmbStatusFilter);
            searchPanel.Controls.Add(lblFilterCat);
            searchPanel.Controls.Add(cmbCategoryFilter);
            searchPanel.Controls.Add(btnRefresh);

            dgvComplaints = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvComplaints);
            dgvComplaints.CellClick += DgvComplaints_CellClick;

            rightPanel.Controls.Add(dgvComplaints);
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
                var dtBuildings = DbHelper.ExecuteDataTable("SELECT BuildingId, BuildingName FROM dbo.Buildings");
                cmbBuilding.DataSource = dtBuildings;
                cmbBuilding.DisplayMember = "BuildingName";
                cmbBuilding.ValueMember = "BuildingId";

                var dtManagers = DbHelper.ExecuteDataTable("SELECT UserId, FullName FROM dbo.Users WHERE Role = 'Building Manager' AND Status = 'Active'");
                var emptyRow = dtManagers.NewRow();
                emptyRow["UserId"] = DBNull.Value;
                emptyRow["FullName"] = "-- Unassigned --";
                dtManagers.Rows.InsertAt(emptyRow, 0);

                cmbAssignedManager.DataSource = dtManagers;
                cmbAssignedManager.DisplayMember = "FullName";
                cmbAssignedManager.ValueMember = "UserId";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading dropdowns: {ex.Message}");
            }
        }

        private void LoadComplaints()
        {
            try
            {
                string query = @"
                    SELECT c.ComplaintId, c.Title, c.Category, c.Priority, c.Status,
                           u.FullName AS SubmittedBy, ISNULL(f.FlatNumber, 'Common Area') AS FlatNumber,
                           b.BuildingName, ISNULL(m.FullName, 'Unassigned') AS AssignedManager,
                           c.SubmittedDate, c.ResolvedDate, c.Description, c.ResolutionNotes,
                           c.UserId, c.BuildingId, c.FlatId, c.AssignedToManagerId
                    FROM dbo.Complaints c
                    INNER JOIN dbo.Users u ON c.UserId = u.UserId
                    INNER JOIN dbo.Buildings b ON c.BuildingId = b.BuildingId
                    LEFT JOIN dbo.Flats f ON c.FlatId = f.FlatId
                    LEFT JOIN dbo.Users m ON c.AssignedToManagerId = m.UserId ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                if (SessionManager.CurrentUser?.Role == "Resident")
                {
                    query += " WHERE c.UserId = @UserId ";
                    parameters.Add(new SqlParameter("@UserId", SessionManager.CurrentUser.UserId));
                }

                query += " ORDER BY c.ComplaintId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvComplaints.DataSource = dt;

                if (dgvComplaints.Columns["ComplaintId"] != null) dgvComplaints.Columns["ComplaintId"].Width = 60;
                if (dgvComplaints.Columns["Description"] != null) dgvComplaints.Columns["Description"].Visible = false;
                if (dgvComplaints.Columns["ResolutionNotes"] != null) dgvComplaints.Columns["ResolutionNotes"].Visible = false;
                if (dgvComplaints.Columns["UserId"] != null) dgvComplaints.Columns["UserId"].Visible = false;
                if (dgvComplaints.Columns["BuildingId"] != null) dgvComplaints.Columns["BuildingId"].Visible = false;
                if (dgvComplaints.Columns["FlatId"] != null) dgvComplaints.Columns["FlatId"].Visible = false;
                if (dgvComplaints.Columns["AssignedToManagerId"] != null) dgvComplaints.Columns["AssignedToManagerId"].Visible = false;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load complaints: {ex.Message}");
            }
        }

        private void FilterComplaints()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string selectedStatus = cmbStatusFilter.SelectedItem?.ToString() ?? "All";
                string selectedCategory = cmbCategoryFilter.SelectedItem?.ToString() ?? "All";

                string query = @"
                    SELECT c.ComplaintId, c.Title, c.Category, c.Priority, c.Status,
                           u.FullName AS SubmittedBy, ISNULL(f.FlatNumber, 'Common Area') AS FlatNumber,
                           b.BuildingName, ISNULL(m.FullName, 'Unassigned') AS AssignedManager,
                           c.SubmittedDate, c.ResolvedDate, c.Description, c.ResolutionNotes,
                           c.UserId, c.BuildingId, c.FlatId, c.AssignedToManagerId
                    FROM dbo.Complaints c
                    INNER JOIN dbo.Users u ON c.UserId = u.UserId
                    INNER JOIN dbo.Buildings b ON c.BuildingId = b.BuildingId
                    LEFT JOIN dbo.Flats f ON c.FlatId = f.FlatId
                    LEFT JOIN dbo.Users m ON c.AssignedToManagerId = m.UserId 
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (SessionManager.CurrentUser?.Role == "Resident")
                {
                    query += " AND c.UserId = @UserId ";
                    parameters.Add(new SqlParameter("@UserId", SessionManager.CurrentUser.UserId));
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (c.Title LIKE @Keyword OR c.Description LIKE @Keyword OR u.FullName LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                if (selectedStatus != "All")
                {
                    query += " AND c.Status = @Status ";
                    parameters.Add(new SqlParameter("@Status", selectedStatus));
                }

                if (selectedCategory != "All")
                {
                    query += " AND c.Category = @Category ";
                    parameters.Add(new SqlParameter("@Category", selectedCategory));
                }

                query += " ORDER BY c.ComplaintId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvComplaints.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvComplaints_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvComplaints.Rows.Count)
            {
                var row = dgvComplaints.Rows[e.RowIndex];
                selectedComplaintId = Convert.ToInt32(row.Cells["ComplaintId"].Value);
                txtTitle.Text = row.Cells["Title"].Value?.ToString() ?? string.Empty;
                txtDescription.Text = row.Cells["Description"].Value?.ToString() ?? string.Empty;
                txtResolutionNotes.Text = row.Cells["ResolutionNotes"].Value?.ToString() ?? string.Empty;

                cmbCategory.SelectedItem = row.Cells["Category"].Value?.ToString() ?? "Other";
                cmbPriority.SelectedItem = row.Cells["Priority"].Value?.ToString() ?? "Medium";
                cmbStatus.SelectedItem = row.Cells["Status"].Value?.ToString() ?? "Pending";

                if (row.Cells["BuildingId"].Value != DBNull.Value)
                    cmbBuilding.SelectedValue = Convert.ToInt32(row.Cells["BuildingId"].Value);

                if (row.Cells["AssignedToManagerId"].Value != DBNull.Value)
                    cmbAssignedManager.SelectedValue = Convert.ToInt32(row.Cells["AssignedToManagerId"].Value);
                else
                    cmbAssignedManager.SelectedIndex = 0;
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                UIHelper.ShowWarning("Please fill in both Complaint Title and Description.");
                return;
            }

            try
            {
                int userId = SessionManager.CurrentUser?.UserId ?? 1;
                int? flatId = SessionManager.ResidentFlatId;
                int buildingId = Convert.ToInt32(cmbBuilding.SelectedValue);

                object mgrId = (cmbAssignedManager.SelectedValue == null || cmbAssignedManager.SelectedValue == DBNull.Value)
                    ? DBNull.Value
                    : cmbAssignedManager.SelectedValue;

                string insertQuery = @"
                    INSERT INTO dbo.Complaints (UserId, FlatId, BuildingId, Category, Title, Description, Priority, Status, AssignedToManagerId)
                    VALUES (@UserId, @FlatId, @BuildingId, @Category, @Title, @Description, @Priority, @Status, @AssignedToManagerId)";

                var parameters = new[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@FlatId", (object?)flatId ?? DBNull.Value),
                    new SqlParameter("@BuildingId", buildingId),
                    new SqlParameter("@Category", cmbCategory.SelectedItem!.ToString()),
                    new SqlParameter("@Title", txtTitle.Text.Trim()),
                    new SqlParameter("@Description", txtDescription.Text.Trim()),
                    new SqlParameter("@Priority", cmbPriority.SelectedItem!.ToString()),
                    new SqlParameter("@Status", cmbStatus.SelectedItem!.ToString()),
                    new SqlParameter("@AssignedToManagerId", mgrId)
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Complaint Submitted Successfully!");
                ClearForm();
                LoadComplaints();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error submitting complaint: {ex.Message}");
            }
        }

        private void BtnUpdateStatus_Click(object? sender, EventArgs e)
        {
            if (selectedComplaintId <= 0)
            {
                UIHelper.ShowWarning("Please select a complaint from the table to update.");
                return;
            }

            try
            {
                string status = cmbStatus.SelectedItem?.ToString() ?? "Pending";
                object mgrId = (cmbAssignedManager.SelectedValue == null || cmbAssignedManager.SelectedValue == DBNull.Value)
                    ? DBNull.Value
                    : cmbAssignedManager.SelectedValue;

                object resolvedDate = (status == "Solved") ? DateTime.Now : DBNull.Value;

                string updateQuery = @"
                    UPDATE dbo.Complaints 
                    SET Status = @Status, AssignedToManagerId = @AssignedToManagerId, 
                        ResolutionNotes = @ResolutionNotes, ResolvedDate = @ResolvedDate
                    WHERE ComplaintId = @ComplaintId";

                var parameters = new[]
                {
                    new SqlParameter("@Status", status),
                    new SqlParameter("@AssignedToManagerId", mgrId),
                    new SqlParameter("@ResolutionNotes", (object?)txtResolutionNotes.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@ResolvedDate", resolvedDate),
                    new SqlParameter("@ComplaintId", selectedComplaintId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Complaint Status Updated Successfully!");
                ClearForm();
                LoadComplaints();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating complaint: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedComplaintId <= 0)
            {
                UIHelper.ShowWarning("Please select a complaint to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this complaint?"))
            {
                return;
            }

            try
            {
                DbHelper.ExecuteNonQuery("DELETE FROM dbo.Complaints WHERE ComplaintId = @ComplaintId",
                    new[] { new SqlParameter("@ComplaintId", selectedComplaintId) });

                UIHelper.ShowSuccess("Complaint Deleted Successfully!");
                ClearForm();
                LoadComplaints();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error deleting complaint: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            selectedComplaintId = 0;
            txtTitle.Clear();
            txtDescription.Clear();
            txtResolutionNotes.Clear();
            cmbCategory.SelectedIndex = 0;
            cmbPriority.SelectedIndex = 1;
            cmbStatus.SelectedIndex = 0;
            if (cmbAssignedManager.Items.Count > 0) cmbAssignedManager.SelectedIndex = 0;
        }
    }
}
