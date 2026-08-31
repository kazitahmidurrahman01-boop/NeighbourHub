using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class NoticeManagementForm : Form
    {
        private int selectedNoticeId = 0;
        private TextBox txtTitle = null!;
        private ComboBox cmbCategory = null!;
        private ComboBox cmbPriority = null!;
        private ComboBox cmbBuilding = null!;
        private DateTimePicker dtpExpiry = null!;
        private TextBox txtContent = null!;
        private CheckBox chkIsActive = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbCategoryFilter = null!;
        private DataGridView dgvNotices = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public NoticeManagementForm()
        {
            InitializeComponent();
            LoadBuildingsDropdown();
            LoadNotices();
        }

        private void InitializeComponent()
        {
            this.Text = "Notice Board & Announcements - NeighbourHub";
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
                Text = "📢 Building Notice Board & Society Announcements",
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
            inputPanel.Controls.Add(CreateLabel("Notice Title *:", 16, top));
            txtTitle = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. Scheduled Water Tank Cleaning" };
            inputPanel.Controls.Add(txtTitle);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Category *:", 16, top));
            cmbCategory = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.AddRange(new object[] { "General", "Maintenance", "Emergency", "Meeting", "Billing" });
            cmbCategory.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbCategory);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Priority *:", 16, top));
            cmbPriority = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPriority.Items.AddRange(new object[] { "Normal", "Important", "Urgent" });
            cmbPriority.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbPriority);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Target Building *:", 16, top));
            cmbBuilding = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbBuilding);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Expiry Date:", 16, top));
            dtpExpiry = new DateTimePicker { Location = new Point(16, top + 22), Size = new Size(300, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(14) };
            inputPanel.Controls.Add(dtpExpiry);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Notice Body / Content *:", 16, top));
            txtContent = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 75), Multiline = true, ScrollBars = ScrollBars.Vertical };
            inputPanel.Controls.Add(txtContent);
            top += 105;

            chkIsActive = new CheckBox { Text = "Is Active & Visible to Residents", Location = new Point(16, top), Size = new Size(300, 24), Checked = true };
            inputPanel.Controls.Add(chkIsActive);
            top += 32;

            // Buttons
            btnAdd = new Button { Text = "➕ Publish", Location = new Point(16, top), Size = new Size(140, 36) };
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
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblSearch = new Label { Text = "Search:", Location = new Point(12, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(65, 11), Size = new Size(200, 28) };
            txtSearch.TextChanged += (s, e) => FilterNotices();

            var lblFilterCat = new Label { Text = "Category:", Location = new Point(280, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbCategoryFilter = new ComboBox { Location = new Point(350, 11), Size = new Size(140, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategoryFilter.Items.AddRange(new object[] { "All", "General", "Maintenance", "Emergency", "Meeting", "Billing" });
            cmbCategoryFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndexChanged += (s, e) => FilterNotices();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(505, 9), Size = new Size(75, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbCategoryFilter.SelectedIndex = 0; LoadNotices(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblFilterCat);
            searchPanel.Controls.Add(cmbCategoryFilter);
            searchPanel.Controls.Add(btnRefresh);

            dgvNotices = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvNotices);
            dgvNotices.CellClick += DgvNotices_CellClick;

            rightPanel.Controls.Add(dgvNotices);
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

        private void LoadNotices()
        {
            try
            {
                string query = @"
                    SELECT n.NoticeId, n.Title, n.Category, n.Priority, b.BuildingName,
                           u.FullName AS PublishedBy, n.PublishedDate, n.ExpiryDate, n.IsActive,
                           n.Content, n.BuildingId, n.PublishedByUserId
                    FROM dbo.Notices n
                    INNER JOIN dbo.Buildings b ON n.BuildingId = b.BuildingId
                    INNER JOIN dbo.Users u ON n.PublishedByUserId = u.UserId
                    ORDER BY n.NoticeId DESC";

                var dt = DbHelper.ExecuteDataTable(query);
                dgvNotices.DataSource = dt;

                if (dgvNotices.Columns["NoticeId"] != null) dgvNotices.Columns["NoticeId"].Width = 60;
                if (dgvNotices.Columns["Content"] != null) dgvNotices.Columns["Content"].Visible = false;
                if (dgvNotices.Columns["BuildingId"] != null) dgvNotices.Columns["BuildingId"].Visible = false;
                if (dgvNotices.Columns["PublishedByUserId"] != null) dgvNotices.Columns["PublishedByUserId"].Visible = false;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load notices: {ex.Message}");
            }
        }

        private void FilterNotices()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string selectedCategory = cmbCategoryFilter.SelectedItem?.ToString() ?? "All";

                string query = @"
                    SELECT n.NoticeId, n.Title, n.Category, n.Priority, b.BuildingName,
                           u.FullName AS PublishedBy, n.PublishedDate, n.ExpiryDate, n.IsActive,
                           n.Content, n.BuildingId, n.PublishedByUserId
                    FROM dbo.Notices n
                    INNER JOIN dbo.Buildings b ON n.BuildingId = b.BuildingId
                    INNER JOIN dbo.Users u ON n.PublishedByUserId = u.UserId
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (n.Title LIKE @Keyword OR n.Content LIKE @Keyword OR b.BuildingName LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                if (selectedCategory != "All")
                {
                    query += " AND n.Category = @Category ";
                    parameters.Add(new SqlParameter("@Category", selectedCategory));
                }

                query += " ORDER BY n.NoticeId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvNotices.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvNotices_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvNotices.Rows.Count)
            {
                var row = dgvNotices.Rows[e.RowIndex];
                selectedNoticeId = Convert.ToInt32(row.Cells["NoticeId"].Value);
                txtTitle.Text = row.Cells["Title"].Value?.ToString() ?? string.Empty;
                txtContent.Text = row.Cells["Content"].Value?.ToString() ?? string.Empty;

                cmbCategory.SelectedItem = row.Cells["Category"].Value?.ToString() ?? "General";
                cmbPriority.SelectedItem = row.Cells["Priority"].Value?.ToString() ?? "Normal";

                if (row.Cells["BuildingId"].Value != DBNull.Value)
                    cmbBuilding.SelectedValue = Convert.ToInt32(row.Cells["BuildingId"].Value);

                if (row.Cells["ExpiryDate"].Value != DBNull.Value)
                    dtpExpiry.Value = Convert.ToDateTime(row.Cells["ExpiryDate"].Value);

                chkIsActive.Checked = Convert.ToBoolean(row.Cells["IsActive"].Value ?? true);
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtContent.Text) || cmbBuilding.SelectedValue == null)
            {
                UIHelper.ShowWarning("Please fill in Title, Content, and Building.");
                return;
            }

            try
            {
                int publisherId = SessionManager.CurrentUser?.UserId ?? 1;

                string insertQuery = @"
                    INSERT INTO dbo.Notices (BuildingId, PublishedByUserId, Title, Content, Category, Priority, ExpiryDate, IsActive)
                    VALUES (@BuildingId, @PublishedByUserId, @Title, @Content, @Category, @Priority, @ExpiryDate, @IsActive)";

                var parameters = new[]
                {
                    new SqlParameter("@BuildingId", Convert.ToInt32(cmbBuilding.SelectedValue)),
                    new SqlParameter("@PublishedByUserId", publisherId),
                    new SqlParameter("@Title", txtTitle.Text.Trim()),
                    new SqlParameter("@Content", txtContent.Text.Trim()),
                    new SqlParameter("@Category", cmbCategory.SelectedItem!.ToString()),
                    new SqlParameter("@Priority", cmbPriority.SelectedItem!.ToString()),
                    new SqlParameter("@ExpiryDate", dtpExpiry.Value.Date),
                    new SqlParameter("@IsActive", chkIsActive.Checked)
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Notice Published Successfully!");
                ClearForm();
                LoadNotices();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error publishing notice: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedNoticeId <= 0)
            {
                UIHelper.ShowWarning("Please select a notice to update.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtContent.Text))
            {
                UIHelper.ShowWarning("Please fill in Title and Content.");
                return;
            }

            try
            {
                string updateQuery = @"
                    UPDATE dbo.Notices 
                    SET BuildingId = @BuildingId, Title = @Title, Content = @Content, 
                        Category = @Category, Priority = @Priority, ExpiryDate = @ExpiryDate, IsActive = @IsActive
                    WHERE NoticeId = @NoticeId";

                var parameters = new[]
                {
                    new SqlParameter("@BuildingId", Convert.ToInt32(cmbBuilding.SelectedValue)),
                    new SqlParameter("@Title", txtTitle.Text.Trim()),
                    new SqlParameter("@Content", txtContent.Text.Trim()),
                    new SqlParameter("@Category", cmbCategory.SelectedItem!.ToString()),
                    new SqlParameter("@Priority", cmbPriority.SelectedItem!.ToString()),
                    new SqlParameter("@ExpiryDate", dtpExpiry.Value.Date),
                    new SqlParameter("@IsActive", chkIsActive.Checked),
                    new SqlParameter("@NoticeId", selectedNoticeId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Notice Updated Successfully!");
                ClearForm();
                LoadNotices();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating notice: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedNoticeId <= 0)
            {
                UIHelper.ShowWarning("Please select a notice to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this notice?"))
            {
                return;
            }

            try
            {
                DbHelper.ExecuteNonQuery("DELETE FROM dbo.Notices WHERE NoticeId = @NoticeId",
                    new[] { new SqlParameter("@NoticeId", selectedNoticeId) });

                UIHelper.ShowSuccess("Notice Deleted Successfully!");
                ClearForm();
                LoadNotices();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error deleting notice: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            selectedNoticeId = 0;
            txtTitle.Clear();
            txtContent.Clear();
            dtpExpiry.Value = DateTime.Today.AddDays(14);
            cmbCategory.SelectedIndex = 0;
            cmbPriority.SelectedIndex = 0;
            chkIsActive.Checked = true;
        }
    }
}
