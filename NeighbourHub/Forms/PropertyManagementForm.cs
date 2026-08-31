using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class PropertyManagementForm : Form
    {
        private int selectedPropertyId = 0;
        private TextBox txtPropertyName = null!;
        private ComboBox cmbPropertyType = null!;
        private TextBox txtAddress = null!;
        private TextBox txtCity = null!;
        private NumericUpDown numTotalFloors = null!;
        private ComboBox cmbOwner = null!;
        private TextBox txtSearch = null!;
        private DataGridView dgvProperties = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public PropertyManagementForm()
        {
            InitializeComponent();
            LoadOwners();
            LoadProperties();
        }

        private void InitializeComponent()
        {
            this.Text = "Property Management - NeighbourHub";
            this.Size = new Size(1000, 640);
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
                Text = "🏙️ Property Management (Apartment Complexes & Estates)",
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
            inputPanel.Controls.Add(CreateLabel("Property Name *:", 16, top));
            txtPropertyName = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtPropertyName);
            top += 58;

            inputPanel.Controls.Add(CreateLabel("Property Type *:", 16, top));
            cmbPropertyType = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPropertyType.Items.AddRange(new object[] { "Residential Complex", "Apartment Tower", "Condominium", "Gated Community", "Mixed Use" });
            cmbPropertyType.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbPropertyType);
            top += 58;

            inputPanel.Controls.Add(CreateLabel("Owner *:", 16, top));
            cmbOwner = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbOwner);
            top += 58;

            inputPanel.Controls.Add(CreateLabel("Address *:", 16, top));
            txtAddress = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtAddress);
            top += 58;

            inputPanel.Controls.Add(CreateLabel("City *:", 16, top));
            txtCity = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), Text = "Dhaka" };
            inputPanel.Controls.Add(txtCity);
            top += 58;

            inputPanel.Controls.Add(CreateLabel("Total Floors:", 16, top));
            numTotalFloors = new NumericUpDown { Location = new Point(16, top + 22), Size = new Size(300, 28), Minimum = 1, Maximum = 100, Value = 6 };
            inputPanel.Controls.Add(numTotalFloors);
            top += 62;

            // Buttons
            btnAdd = new Button { Text = "➕ Add Property", Location = new Point(16, top), Size = new Size(140, 36) };
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

            var lblSearch = new Label { Text = "Search Property:", Location = new Point(12, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(135, 11), Size = new Size(250, 28) };
            txtSearch.TextChanged += (s, e) => FilterProperties();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(400, 9), Size = new Size(80, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); LoadProperties(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(btnRefresh);

            dgvProperties = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvProperties);
            dgvProperties.CellClick += DgvProperties_CellClick;

            rightPanel.Controls.Add(dgvProperties);
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

        private void LoadOwners()
        {
            try
            {
                var dt = DbHelper.ExecuteDataTable("SELECT UserId, FullName + ' (' + Username + ')' AS DisplayName FROM dbo.Users WHERE Role IN ('Property Owner', 'Admin') AND Status = 'Active'");
                cmbOwner.DataSource = dt;
                cmbOwner.DisplayMember = "DisplayName";
                cmbOwner.ValueMember = "UserId";

                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    cmbOwner.SelectedValue = SessionManager.CurrentUser.UserId;
                    cmbOwner.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading owners: {ex.Message}");
            }
        }

        private void LoadProperties()
        {
            try
            {
                string query = @"
                    SELECT p.PropertyId, p.PropertyName, p.PropertyType, u.FullName AS OwnerName, 
                           p.Address, p.City, p.TotalFloors, p.CreatedAt, p.OwnerUserId
                    FROM dbo.Properties p
                    INNER JOIN dbo.Users u ON p.OwnerUserId = u.UserId ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    query += " WHERE p.OwnerUserId = @OwnerId ";
                    parameters.Add(new SqlParameter("@OwnerId", SessionManager.CurrentUser.UserId));
                }

                query += " ORDER BY p.PropertyId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvProperties.DataSource = dt;

                if (dgvProperties.Columns["PropertyId"] != null) dgvProperties.Columns["PropertyId"].Width = 60;
                if (dgvProperties.Columns["OwnerUserId"] != null) dgvProperties.Columns["OwnerUserId"].Visible = false;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load properties: {ex.Message}");
            }
        }

        private void FilterProperties()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string query = @"
                    SELECT p.PropertyId, p.PropertyName, p.PropertyType, u.FullName AS OwnerName, 
                           p.Address, p.City, p.TotalFloors, p.CreatedAt, p.OwnerUserId
                    FROM dbo.Properties p
                    INNER JOIN dbo.Users u ON p.OwnerUserId = u.UserId
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    query += " AND p.OwnerUserId = @OwnerId ";
                    parameters.Add(new SqlParameter("@OwnerId", SessionManager.CurrentUser.UserId));
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (p.PropertyName LIKE @Keyword OR p.Address LIKE @Keyword OR p.City LIKE @Keyword OR u.FullName LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                query += " ORDER BY p.PropertyId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvProperties.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvProperties_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvProperties.Rows.Count)
            {
                var row = dgvProperties.Rows[e.RowIndex];
                selectedPropertyId = Convert.ToInt32(row.Cells["PropertyId"].Value);
                txtPropertyName.Text = row.Cells["PropertyName"].Value?.ToString() ?? string.Empty;
                cmbPropertyType.SelectedItem = row.Cells["PropertyType"].Value?.ToString() ?? "Residential Complex";
                txtAddress.Text = row.Cells["Address"].Value?.ToString() ?? string.Empty;
                txtCity.Text = row.Cells["City"].Value?.ToString() ?? "Dhaka";
                numTotalFloors.Value = Convert.ToDecimal(row.Cells["TotalFloors"].Value ?? 1);

                if (cmbOwner.Enabled && row.Cells["OwnerUserId"].Value != DBNull.Value)
                {
                    cmbOwner.SelectedValue = Convert.ToInt32(row.Cells["OwnerUserId"].Value);
                }
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPropertyName.Text) || string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                UIHelper.ShowWarning("Please fill all required fields (Property Name, Address).");
                return;
            }

            try
            {
                int ownerId = SessionManager.CurrentUser?.Role == "Property Owner"
                    ? SessionManager.CurrentUser.UserId
                    : Convert.ToInt32(cmbOwner.SelectedValue);

                string insertQuery = @"
                    INSERT INTO dbo.Properties (OwnerUserId, PropertyName, PropertyType, Address, City, TotalFloors)
                    VALUES (@OwnerUserId, @PropertyName, @PropertyType, @Address, @City, @TotalFloors)";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerUserId", ownerId),
                    new SqlParameter("@PropertyName", txtPropertyName.Text.Trim()),
                    new SqlParameter("@PropertyType", cmbPropertyType.SelectedItem!.ToString()),
                    new SqlParameter("@Address", txtAddress.Text.Trim()),
                    new SqlParameter("@City", txtCity.Text.Trim()),
                    new SqlParameter("@TotalFloors", (int)numTotalFloors.Value)
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Data Saved Successfully!");
                ClearForm();
                LoadProperties();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error adding property: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedPropertyId <= 0)
            {
                UIHelper.ShowWarning("Please select a property from the table to update.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPropertyName.Text) || string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                UIHelper.ShowWarning("Please fill all required fields (Property Name, Address).");
                return;
            }

            try
            {
                int ownerId = SessionManager.CurrentUser?.Role == "Property Owner"
                    ? SessionManager.CurrentUser.UserId
                    : Convert.ToInt32(cmbOwner.SelectedValue);

                string updateQuery = @"
                    UPDATE dbo.Properties 
                    SET OwnerUserId = @OwnerUserId, PropertyName = @PropertyName, 
                        PropertyType = @PropertyType, Address = @Address, City = @City, TotalFloors = @TotalFloors
                    WHERE PropertyId = @PropertyId";

                var parameters = new[]
                {
                    new SqlParameter("@OwnerUserId", ownerId),
                    new SqlParameter("@PropertyName", txtPropertyName.Text.Trim()),
                    new SqlParameter("@PropertyType", cmbPropertyType.SelectedItem!.ToString()),
                    new SqlParameter("@Address", txtAddress.Text.Trim()),
                    new SqlParameter("@City", txtCity.Text.Trim()),
                    new SqlParameter("@TotalFloors", (int)numTotalFloors.Value),
                    new SqlParameter("@PropertyId", selectedPropertyId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Data Updated Successfully!");
                ClearForm();
                LoadProperties();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating property: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedPropertyId <= 0)
            {
                UIHelper.ShowWarning("Please select a property to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this property? All associated buildings and flats will be affected."))
            {
                return;
            }

            try
            {
                string deleteQuery = "DELETE FROM dbo.Properties WHERE PropertyId = @PropertyId";
                DbHelper.ExecuteNonQuery(deleteQuery, new[] { new SqlParameter("@PropertyId", selectedPropertyId) });

                UIHelper.ShowSuccess("Data Deleted Successfully!");
                ClearForm();
                LoadProperties();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Cannot delete property. It contains buildings or flats.\nDetails: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            selectedPropertyId = 0;
            txtPropertyName.Clear();
            txtAddress.Clear();
            txtCity.Text = "Dhaka";
            numTotalFloors.Value = 6;
            cmbPropertyType.SelectedIndex = 0;
        }
    }
}
