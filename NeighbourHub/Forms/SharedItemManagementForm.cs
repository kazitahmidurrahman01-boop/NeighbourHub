using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class SharedItemManagementForm : Form
    {
        private int selectedItemId = 0;
        private TextBox txtItemName = null!;
        private ComboBox cmbCategory = null!;
        private TextBox txtDescription = null!;
        private ComboBox cmbAvailability = null!;
        private TextBox txtContactNumber = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbCategoryFilter = null!;
        private DataGridView dgvItems = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public SharedItemManagementForm()
        {
            InitializeComponent();
            LoadItems();
        }

        private void InitializeComponent()
        {
            this.Text = "Community Item Sharing & Tool Library - NeighbourHub";
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
                Text = "🤝 Community Item Sharing & Tool Lending Library",
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
            inputPanel.Controls.Add(CreateLabel("Item / Tool Name *:", 16, top));
            txtItemName = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. Bosch Impact Drill 650W" };
            inputPanel.Controls.Add(txtItemName);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Category *:", 16, top));
            cmbCategory = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.AddRange(new object[] { "Tools", "Appliances", "Ladders", "Sports/Games", "Books", "Other" });
            cmbCategory.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbCategory);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Contact Phone Number *:", 16, top));
            txtContactNumber = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. 01812345678" };
            inputPanel.Controls.Add(txtContactNumber);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Availability Status *:", 16, top));
            cmbAvailability = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAvailability.Items.AddRange(new object[] { "Available", "Borrowed", "Unavailable" });
            cmbAvailability.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbAvailability);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Item Description & Usage Guidelines:", 16, top));
            txtDescription = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 80), Multiline = true, ScrollBars = ScrollBars.Vertical, PlaceholderText = "e.g. Includes drill bits set. Please return clean." };
            inputPanel.Controls.Add(txtDescription);
            top += 110;

            // Buttons
            btnAdd = new Button { Text = "➕ Share Item", Location = new Point(16, top), Size = new Size(140, 36) };
            UIHelper.StyleButton(btnAdd, ThemeColors.Primary, Color.White);
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button { Text = "💾 Update", Location = new Point(165, top), Size = new Size(150, 36) };
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

            var lblSearch = new Label { Text = "Search Items:", Location = new Point(12, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(110, 11), Size = new Size(180, 28) };
            txtSearch.TextChanged += (s, e) => FilterItems();

            var lblFilterCat = new Label { Text = "Category:", Location = new Point(300, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbCategoryFilter = new ComboBox { Location = new Point(370, 11), Size = new Size(130, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategoryFilter.Items.AddRange(new object[] { "All", "Tools", "Appliances", "Ladders", "Sports/Games", "Books", "Other" });
            cmbCategoryFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndexChanged += (s, e) => FilterItems();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(515, 9), Size = new Size(75, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbCategoryFilter.SelectedIndex = 0; LoadItems(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblFilterCat);
            searchPanel.Controls.Add(cmbCategoryFilter);
            searchPanel.Controls.Add(btnRefresh);

            dgvItems = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvItems);
            dgvItems.CellClick += DgvItems_CellClick;

            rightPanel.Controls.Add(dgvItems);
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

        private void LoadItems()
        {
            try
            {
                string query = @"
                    SELECT s.ItemId, s.ItemName, s.Category, s.AvailabilityStatus, 
                           u.FullName AS OwnerName, s.ContactNumber, s.Description, s.CreatedAt, s.OwnerUserId
                    FROM dbo.SharedItems s
                    INNER JOIN dbo.Users u ON s.OwnerUserId = u.UserId
                    ORDER BY s.ItemId DESC";

                var dt = DbHelper.ExecuteDataTable(query);
                dgvItems.DataSource = dt;

                if (dgvItems.Columns["ItemId"] != null) dgvItems.Columns["ItemId"].Width = 60;
                if (dgvItems.Columns["OwnerUserId"] != null) dgvItems.Columns["OwnerUserId"].Visible = false;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load shared items: {ex.Message}");
            }
        }

        private void FilterItems()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string selectedCategory = cmbCategoryFilter.SelectedItem?.ToString() ?? "All";

                string query = @"
                    SELECT s.ItemId, s.ItemName, s.Category, s.AvailabilityStatus, 
                           u.FullName AS OwnerName, s.ContactNumber, s.Description, s.CreatedAt, s.OwnerUserId
                    FROM dbo.SharedItems s
                    INNER JOIN dbo.Users u ON s.OwnerUserId = u.UserId
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (s.ItemName LIKE @Keyword OR s.Description LIKE @Keyword OR u.FullName LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                if (selectedCategory != "All")
                {
                    query += " AND s.Category = @Category ";
                    parameters.Add(new SqlParameter("@Category", selectedCategory));
                }

                query += " ORDER BY s.ItemId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvItems.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvItems_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvItems.Rows.Count)
            {
                var row = dgvItems.Rows[e.RowIndex];
                selectedItemId = Convert.ToInt32(row.Cells["ItemId"].Value);
                txtItemName.Text = row.Cells["ItemName"].Value?.ToString() ?? string.Empty;
                txtDescription.Text = row.Cells["Description"].Value?.ToString() ?? string.Empty;
                txtContactNumber.Text = row.Cells["ContactNumber"].Value?.ToString() ?? string.Empty;

                cmbCategory.SelectedItem = row.Cells["Category"].Value?.ToString() ?? "Tools";
                cmbAvailability.SelectedItem = row.Cells["AvailabilityStatus"].Value?.ToString() ?? "Available";
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtItemName.Text) || string.IsNullOrWhiteSpace(txtContactNumber.Text))
            {
                UIHelper.ShowWarning("Please fill in Item Name and Contact Phone Number.");
                return;
            }

            try
            {
                int ownerUserId = SessionManager.CurrentUser?.UserId ?? 1;

                string insertQuery = @"
                    INSERT INTO dbo.SharedItems (OwnerUserId, ItemName, Category, Description, AvailabilityStatus, ContactNumber)
                    VALUES (@OwnerUserId, @ItemName, @Category, @Description, @AvailabilityStatus, @ContactNumber)";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerUserId", ownerUserId),
                    new SqlParameter("@ItemName", txtItemName.Text.Trim()),
                    new SqlParameter("@Category", cmbCategory.SelectedItem!.ToString()),
                    new SqlParameter("@Description", (object?)txtDescription.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@AvailabilityStatus", cmbAvailability.SelectedItem!.ToString()),
                    new SqlParameter("@ContactNumber", txtContactNumber.Text.Trim())
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Item Listed for Community Sharing!");
                ClearForm();
                LoadItems();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error sharing item: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedItemId <= 0)
            {
                UIHelper.ShowWarning("Please select an item from the table to update.");
                return;
            }

            try
            {
                string updateQuery = @"
                    UPDATE dbo.SharedItems 
                    SET ItemName = @ItemName, Category = @Category, Description = @Description, 
                        AvailabilityStatus = @AvailabilityStatus, ContactNumber = @ContactNumber
                    WHERE ItemId = @ItemId";

                var parameters = new[]
                {
                    new SqlParameter("@ItemName", txtItemName.Text.Trim()),
                    new SqlParameter("@Category", cmbCategory.SelectedItem!.ToString()),
                    new SqlParameter("@Description", (object?)txtDescription.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@AvailabilityStatus", cmbAvailability.SelectedItem!.ToString()),
                    new SqlParameter("@ContactNumber", txtContactNumber.Text.Trim()),
                    new SqlParameter("@ItemId", selectedItemId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Shared Item Updated Successfully!");
                ClearForm();
                LoadItems();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating item: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedItemId <= 0)
            {
                UIHelper.ShowWarning("Please select an item to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to remove this item listing?"))
            {
                return;
            }

            try
            {
                DbHelper.ExecuteNonQuery("DELETE FROM dbo.SharedItems WHERE ItemId = @ItemId",
                    new[] { new SqlParameter("@ItemId", selectedItemId) });

                UIHelper.ShowSuccess("Item Listing Deleted Successfully!");
                ClearForm();
                LoadItems();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error deleting item: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            selectedItemId = 0;
            txtItemName.Clear();
            txtDescription.Clear();
            txtContactNumber.Clear();
            cmbCategory.SelectedIndex = 0;
            cmbAvailability.SelectedIndex = 0;
        }
    }
}
