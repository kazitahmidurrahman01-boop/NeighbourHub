using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class BuildingManagementForm : Form
    {
        private int selectedBuildingId = 0;
        private TextBox txtBuildingName = null!;
        private TextBox txtBuildingCode = null!;
        private ComboBox cmbProperty = null!;
        private ComboBox cmbManager = null!;
        private NumericUpDown numTotalUnits = null!;
        private TextBox txtAddress = null!;
        private TextBox txtSecurityContact = null!;
        private TextBox txtSearch = null!;
        private DataGridView dgvBuildings = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public BuildingManagementForm()
        {
            InitializeComponent();
            LoadDropdowns();
            LoadBuildings();
        }

        private void InitializeComponent()
        {
            this.Text = "Building Management - NeighbourHub";
            this.Size = new Size(1020, 660);
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
                Text = "🏢 Building & Block Management",
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
            inputPanel.Controls.Add(CreateLabel("Building Name *:", 16, top));
            txtBuildingName = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtBuildingName);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Building / Block Code:", 16, top));
            txtBuildingCode = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. SKY-A, BLK-1" };
            inputPanel.Controls.Add(txtBuildingCode);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Property Complex *:", 16, top));
            cmbProperty = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbProperty);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Assigned Building Manager:", 16, top));
            cmbManager = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbManager);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Total Units (Flats):", 16, top));
            numTotalUnits = new NumericUpDown { Location = new Point(16, top + 22), Size = new Size(300, 28), Minimum = 1, Maximum = 500, Value = 12 };
            inputPanel.Controls.Add(numTotalUnits);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Address:", 16, top));
            txtAddress = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtAddress);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Security / Gate Contact:", 16, top));
            txtSecurityContact = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. 01700112233" };
            inputPanel.Controls.Add(txtSecurityContact);
            top += 58;

            // Buttons
            btnAdd = new Button { Text = "➕ Add Building", Location = new Point(16, top), Size = new Size(140, 36) };
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

            var lblSearch = new Label { Text = "Search Building:", Location = new Point(12, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(135, 11), Size = new Size(250, 28) };
            txtSearch.TextChanged += (s, e) => FilterBuildings();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(400, 9), Size = new Size(80, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); LoadBuildings(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(btnRefresh);

            dgvBuildings = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvBuildings);
            dgvBuildings.CellClick += DgvBuildings_CellClick;

            rightPanel.Controls.Add(dgvBuildings);
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
                // Properties dropdown
                string propQuery = "SELECT PropertyId, PropertyName FROM dbo.Properties";
                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    propQuery += $" WHERE OwnerUserId = {SessionManager.CurrentUser.UserId}";
                }
                var dtProps = DbHelper.ExecuteDataTable(propQuery);
                cmbProperty.DataSource = dtProps;
                cmbProperty.DisplayMember = "PropertyName";
                cmbProperty.ValueMember = "PropertyId";

                // Managers dropdown
                var dtManagers = DbHelper.ExecuteDataTable("SELECT UserId, FullName + ' (' + Phone + ')' AS ManagerDisplay FROM dbo.Users WHERE Role = 'Building Manager' AND Status = 'Active'");
                
                // Add unassigned option
                var emptyRow = dtManagers.NewRow();
                emptyRow["UserId"] = DBNull.Value;
                emptyRow["ManagerDisplay"] = "-- Unassigned --";
                dtManagers.Rows.InsertAt(emptyRow, 0);

                cmbManager.DataSource = dtManagers;
                cmbManager.DisplayMember = "ManagerDisplay";
                cmbManager.ValueMember = "UserId";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading dropdowns: {ex.Message}");
            }
        }

        private void LoadBuildings()
        {
            try
            {
                string query = @"
                    SELECT b.BuildingId, b.BuildingName, b.BuildingCode, p.PropertyName, 
                           ISNULL(u.FullName, 'Unassigned') AS ManagerName, b.TotalUnits, 
                           b.SecurityContact, b.Address, b.PropertyId, b.ManagerUserId
                    FROM dbo.Buildings b
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId
                    LEFT JOIN dbo.Users u ON b.ManagerUserId = u.UserId ";

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

                query += " ORDER BY b.BuildingId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvBuildings.DataSource = dt;

                if (dgvBuildings.Columns["BuildingId"] != null) dgvBuildings.Columns["BuildingId"].Width = 60;
                if (dgvBuildings.Columns["PropertyId"] != null) dgvBuildings.Columns["PropertyId"].Visible = false;
                if (dgvBuildings.Columns["ManagerUserId"] != null) dgvBuildings.Columns["ManagerUserId"].Visible = false;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load buildings: {ex.Message}");
            }
        }

        private void FilterBuildings()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string query = @"
                    SELECT b.BuildingId, b.BuildingName, b.BuildingCode, p.PropertyName, 
                           ISNULL(u.FullName, 'Unassigned') AS ManagerName, b.TotalUnits, 
                           b.SecurityContact, b.Address, b.PropertyId, b.ManagerUserId
                    FROM dbo.Buildings b
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId
                    LEFT JOIN dbo.Users u ON b.ManagerUserId = u.UserId
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    query += " AND p.OwnerUserId = @OwnerId ";
                    parameters.Add(new SqlParameter("@OwnerId", SessionManager.CurrentUser.UserId));
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (b.BuildingName LIKE @Keyword OR b.BuildingCode LIKE @Keyword OR p.PropertyName LIKE @Keyword OR u.FullName LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                query += " ORDER BY b.BuildingId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvBuildings.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvBuildings_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvBuildings.Rows.Count)
            {
                var row = dgvBuildings.Rows[e.RowIndex];
                selectedBuildingId = Convert.ToInt32(row.Cells["BuildingId"].Value);
                txtBuildingName.Text = row.Cells["BuildingName"].Value?.ToString() ?? string.Empty;
                txtBuildingCode.Text = row.Cells["BuildingCode"].Value?.ToString() ?? string.Empty;
                txtAddress.Text = row.Cells["Address"].Value?.ToString() ?? string.Empty;
                txtSecurityContact.Text = row.Cells["SecurityContact"].Value?.ToString() ?? string.Empty;
                numTotalUnits.Value = Convert.ToDecimal(row.Cells["TotalUnits"].Value ?? 0);

                if (row.Cells["PropertyId"].Value != DBNull.Value)
                    cmbProperty.SelectedValue = Convert.ToInt32(row.Cells["PropertyId"].Value);

                if (row.Cells["ManagerUserId"].Value != DBNull.Value)
                    cmbManager.SelectedValue = Convert.ToInt32(row.Cells["ManagerUserId"].Value);
                else
                    cmbManager.SelectedIndex = 0;
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBuildingName.Text) || cmbProperty.SelectedValue == null)
            {
                UIHelper.ShowWarning("Please fill all required fields (Building Name and Property).");
                return;
            }

            try
            {
                object mgrId = (cmbManager.SelectedValue == null || cmbManager.SelectedValue == DBNull.Value) 
                    ? DBNull.Value 
                    : cmbManager.SelectedValue;

                string insertQuery = @"
                    INSERT INTO dbo.Buildings (PropertyId, ManagerUserId, BuildingName, BuildingCode, TotalUnits, Address, SecurityContact)
                    VALUES (@PropertyId, @ManagerUserId, @BuildingName, @BuildingCode, @TotalUnits, @Address, @SecurityContact)";

                var parameters = new[]
                {
                    new SqlParameter("@PropertyId", Convert.ToInt32(cmbProperty.SelectedValue)),
                    new SqlParameter("@ManagerUserId", mgrId),
                    new SqlParameter("@BuildingName", txtBuildingName.Text.Trim()),
                    new SqlParameter("@BuildingCode", (object?)txtBuildingCode.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@TotalUnits", (int)numTotalUnits.Value),
                    new SqlParameter("@Address", (object?)txtAddress.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@SecurityContact", (object?)txtSecurityContact.Text.Trim() ?? DBNull.Value)
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Data Saved Successfully!");
                ClearForm();
                LoadBuildings();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error adding building: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedBuildingId <= 0)
            {
                UIHelper.ShowWarning("Please select a building from the table to update.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBuildingName.Text) || cmbProperty.SelectedValue == null)
            {
                UIHelper.ShowWarning("Please fill all required fields (Building Name and Property).");
                return;
            }

            try
            {
                object mgrId = (cmbManager.SelectedValue == null || cmbManager.SelectedValue == DBNull.Value)
                    ? DBNull.Value
                    : cmbManager.SelectedValue;

                string updateQuery = @"
                    UPDATE dbo.Buildings 
                    SET PropertyId = @PropertyId, ManagerUserId = @ManagerUserId, BuildingName = @BuildingName, 
                        BuildingCode = @BuildingCode, TotalUnits = @TotalUnits, Address = @Address, SecurityContact = @SecurityContact
                    WHERE BuildingId = @BuildingId";

                var parameters = new[]
                {
                    new SqlParameter("@PropertyId", Convert.ToInt32(cmbProperty.SelectedValue)),
                    new SqlParameter("@ManagerUserId", mgrId),
                    new SqlParameter("@BuildingName", txtBuildingName.Text.Trim()),
                    new SqlParameter("@BuildingCode", (object?)txtBuildingCode.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@TotalUnits", (int)numTotalUnits.Value),
                    new SqlParameter("@Address", (object?)txtAddress.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@SecurityContact", (object?)txtSecurityContact.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@BuildingId", selectedBuildingId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Data Updated Successfully!");
                ClearForm();
                LoadBuildings();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating building: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedBuildingId <= 0)
            {
                UIHelper.ShowWarning("Please select a building to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this building? All associated flats will be affected."))
            {
                return;
            }

            try
            {
                string deleteQuery = "DELETE FROM dbo.Buildings WHERE BuildingId = @BuildingId";
                DbHelper.ExecuteNonQuery(deleteQuery, new[] { new SqlParameter("@BuildingId", selectedBuildingId) });

                UIHelper.ShowSuccess("Data Deleted Successfully!");
                ClearForm();
                LoadBuildings();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Cannot delete building. It has flats or records attached.\nDetails: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            selectedBuildingId = 0;
            txtBuildingName.Clear();
            txtBuildingCode.Clear();
            txtAddress.Clear();
            txtSecurityContact.Clear();
            numTotalUnits.Value = 12;
            if (cmbProperty.Items.Count > 0) cmbProperty.SelectedIndex = 0;
            if (cmbManager.Items.Count > 0) cmbManager.SelectedIndex = 0;
        }
    }
}
