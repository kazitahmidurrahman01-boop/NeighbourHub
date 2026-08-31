using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class FlatManagementForm : Form
    {
        private int selectedFlatId = 0;
        private TextBox txtFlatNumber = null!;
        private NumericUpDown numFloor = null!;
        private ComboBox cmbBuilding = null!;
        private NumericUpDown numBedrooms = null!;
        private NumericUpDown numBathrooms = null!;
        private NumericUpDown numAreaSqFt = null!;
        private TextBox txtMonthlyRent = null!;
        private ComboBox cmbStatus = null!;
        private TextBox txtDescription = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbStatusFilter = null!;
        private DataGridView dgvFlats = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public FlatManagementForm()
        {
            InitializeComponent();
            LoadBuildingsDropdown();
            LoadFlats();
        }

        private void InitializeComponent()
        {
            this.Text = "Flat & Unit Management - NeighbourHub";
            this.Size = new Size(1060, 700);
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
                Text = "🚪 Flat & Apartment Unit Management",
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
            inputPanel.Controls.Add(CreateLabel("Flat Number * (e.g. 5A, 102):", 16, top));
            txtFlatNumber = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtFlatNumber);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Building *:", 16, top));
            cmbBuilding = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbBuilding);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Floor Number *:", 16, top));
            numFloor = new NumericUpDown { Location = new Point(16, top + 22), Size = new Size(300, 28), Minimum = 0, Maximum = 100, Value = 1 };
            inputPanel.Controls.Add(numFloor);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Bedrooms & Bathrooms:", 16, top));
            numBedrooms = new NumericUpDown { Location = new Point(16, top + 22), Size = new Size(140, 28), Minimum = 1, Maximum = 10, Value = 2 };
            numBathrooms = new NumericUpDown { Location = new Point(176, top + 22), Size = new Size(140, 28), Minimum = 1, Maximum = 10, Value = 2 };
            inputPanel.Controls.Add(numBedrooms);
            inputPanel.Controls.Add(numBathrooms);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Area (Sq Ft):", 16, top));
            numAreaSqFt = new NumericUpDown { Location = new Point(16, top + 22), Size = new Size(300, 28), Minimum = 100, Maximum = 10000, Value = 1200 };
            inputPanel.Controls.Add(numAreaSqFt);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Monthly Rent (৳ BDT) *:", 16, top));
            txtMonthlyRent = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), Text = "15000" };
            inputPanel.Controls.Add(txtMonthlyRent);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Flat Status *:", 16, top));
            cmbStatus = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Vacant", "Occupied", "Under Maintenance" });
            cmbStatus.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbStatus);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Description / Amenities:", 16, top));
            txtDescription = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. Balcony, South facing, Tile floor" };
            inputPanel.Controls.Add(txtDescription);
            top += 58;

            // Buttons
            btnAdd = new Button { Text = "➕ Add Flat", Location = new Point(16, top), Size = new Size(140, 36) };
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

            var lblSearch = new Label { Text = "Search Flat:", Location = new Point(12, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(105, 11), Size = new Size(180, 28) };
            txtSearch.TextChanged += (s, e) => FilterFlats();

            var lblFilterStatus = new Label { Text = "Status:", Location = new Point(300, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbStatusFilter = new ComboBox { Location = new Point(360, 11), Size = new Size(160, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatusFilter.Items.AddRange(new object[] { "All Statuses", "Vacant", "Occupied", "Under Maintenance" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => FilterFlats();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(535, 9), Size = new Size(80, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbStatusFilter.SelectedIndex = 0; LoadFlats(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblFilterStatus);
            searchPanel.Controls.Add(cmbStatusFilter);
            searchPanel.Controls.Add(btnRefresh);

            dgvFlats = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvFlats);
            dgvFlats.CellClick += DgvFlats_CellClick;

            rightPanel.Controls.Add(dgvFlats);
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
                string query = @"
                    SELECT b.BuildingId, b.BuildingName + ' (' + p.PropertyName + ')' AS BuildingDisplay
                    FROM dbo.Buildings b
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId";

                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    query += $" WHERE p.OwnerUserId = {SessionManager.CurrentUser.UserId}";
                }
                else if (SessionManager.CurrentUser?.Role == "Building Manager")
                {
                    query += $" WHERE b.ManagerUserId = {SessionManager.CurrentUser.UserId}";
                }

                var dt = DbHelper.ExecuteDataTable(query);
                cmbBuilding.DataSource = dt;
                cmbBuilding.DisplayMember = "BuildingDisplay";
                cmbBuilding.ValueMember = "BuildingId";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading buildings: {ex.Message}");
            }
        }

        private void LoadFlats()
        {
            try
            {
                string query = @"
                    SELECT f.FlatId, f.FlatNumber, f.FloorNumber, b.BuildingName, p.PropertyName,
                           f.Bedrooms, f.Bathrooms, f.AreaSqFt, f.MonthlyRent, f.Status,
                           ISNULL(tu.FullName, 'None') AS TenantName,
                           f.BuildingId, f.Description
                    FROM dbo.Flats f
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId
                    LEFT JOIN dbo.Tenants t ON f.FlatId = t.FlatId AND t.Status = 'Active'
                    LEFT JOIN dbo.Users tu ON t.UserId = tu.UserId ";

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

                query += " ORDER BY f.BuildingId, f.FloorNumber, f.FlatNumber";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvFlats.DataSource = dt;

                if (dgvFlats.Columns["FlatId"] != null) dgvFlats.Columns["FlatId"].Width = 60;
                if (dgvFlats.Columns["BuildingId"] != null) dgvFlats.Columns["BuildingId"].Visible = false;
                if (dgvFlats.Columns["Description"] != null) dgvFlats.Columns["Description"].Visible = false;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load flats: {ex.Message}");
            }
        }

        private void FilterFlats()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string selectedStatus = cmbStatusFilter.SelectedItem?.ToString() ?? "All Statuses";

                string query = @"
                    SELECT f.FlatId, f.FlatNumber, f.FloorNumber, b.BuildingName, p.PropertyName,
                           f.Bedrooms, f.Bathrooms, f.AreaSqFt, f.MonthlyRent, f.Status,
                           ISNULL(tu.FullName, 'None') AS TenantName,
                           f.BuildingId, f.Description
                    FROM dbo.Flats f
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId
                    LEFT JOIN dbo.Tenants t ON f.FlatId = t.FlatId AND t.Status = 'Active'
                    LEFT JOIN dbo.Users tu ON t.UserId = tu.UserId 
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
                    query += " AND (f.FlatNumber LIKE @Keyword OR b.BuildingName LIKE @Keyword OR p.PropertyName LIKE @Keyword OR tu.FullName LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                if (selectedStatus != "All Statuses")
                {
                    query += " AND f.Status = @Status ";
                    parameters.Add(new SqlParameter("@Status", selectedStatus));
                }

                query += " ORDER BY f.BuildingId, f.FloorNumber, f.FlatNumber";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvFlats.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvFlats_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvFlats.Rows.Count)
            {
                var row = dgvFlats.Rows[e.RowIndex];
                selectedFlatId = Convert.ToInt32(row.Cells["FlatId"].Value);
                txtFlatNumber.Text = row.Cells["FlatNumber"].Value?.ToString() ?? string.Empty;
                numFloor.Value = Convert.ToDecimal(row.Cells["FloorNumber"].Value ?? 1);
                numBedrooms.Value = Convert.ToDecimal(row.Cells["Bedrooms"].Value ?? 2);
                numBathrooms.Value = Convert.ToDecimal(row.Cells["Bathrooms"].Value ?? 2);
                numAreaSqFt.Value = Convert.ToDecimal(row.Cells["AreaSqFt"].Value ?? 1000);
                txtMonthlyRent.Text = row.Cells["MonthlyRent"].Value?.ToString() ?? "0";
                txtDescription.Text = row.Cells["Description"].Value?.ToString() ?? string.Empty;

                if (row.Cells["BuildingId"].Value != DBNull.Value)
                    cmbBuilding.SelectedValue = Convert.ToInt32(row.Cells["BuildingId"].Value);

                string status = row.Cells["Status"].Value?.ToString() ?? "Vacant";
                cmbStatus.SelectedItem = status;
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                string insertQuery = @"
                    INSERT INTO dbo.Flats (BuildingId, FlatNumber, FloorNumber, Bedrooms, Bathrooms, AreaSqFt, MonthlyRent, Status, Description)
                    VALUES (@BuildingId, @FlatNumber, @FloorNumber, @Bedrooms, @Bathrooms, @AreaSqFt, @MonthlyRent, @Status, @Description)";

                var parameters = new[]
                {
                    new SqlParameter("@BuildingId", Convert.ToInt32(cmbBuilding.SelectedValue)),
                    new SqlParameter("@FlatNumber", txtFlatNumber.Text.Trim()),
                    new SqlParameter("@FloorNumber", (int)numFloor.Value),
                    new SqlParameter("@Bedrooms", (int)numBedrooms.Value),
                    new SqlParameter("@Bathrooms", (int)numBathrooms.Value),
                    new SqlParameter("@AreaSqFt", numAreaSqFt.Value),
                    new SqlParameter("@MonthlyRent", decimal.Parse(txtMonthlyRent.Text.Trim())),
                    new SqlParameter("@Status", cmbStatus.SelectedItem!.ToString()),
                    new SqlParameter("@Description", (object?)txtDescription.Text.Trim() ?? DBNull.Value)
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Data Saved Successfully!");
                ClearForm();
                LoadFlats();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error adding flat: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedFlatId <= 0)
            {
                UIHelper.ShowWarning("Please select a flat from the table to update.");
                return;
            }

            if (!ValidateInputs()) return;

            try
            {
                string updateQuery = @"
                    UPDATE dbo.Flats 
                    SET BuildingId = @BuildingId, FlatNumber = @FlatNumber, FloorNumber = @FloorNumber, 
                        Bedrooms = @Bedrooms, Bathrooms = @Bathrooms, AreaSqFt = @AreaSqFt, 
                        MonthlyRent = @MonthlyRent, Status = @Status, Description = @Description
                    WHERE FlatId = @FlatId";

                var parameters = new[]
                {
                    new SqlParameter("@BuildingId", Convert.ToInt32(cmbBuilding.SelectedValue)),
                    new SqlParameter("@FlatNumber", txtFlatNumber.Text.Trim()),
                    new SqlParameter("@FloorNumber", (int)numFloor.Value),
                    new SqlParameter("@Bedrooms", (int)numBedrooms.Value),
                    new SqlParameter("@Bathrooms", (int)numBathrooms.Value),
                    new SqlParameter("@AreaSqFt", numAreaSqFt.Value),
                    new SqlParameter("@MonthlyRent", decimal.Parse(txtMonthlyRent.Text.Trim())),
                    new SqlParameter("@Status", cmbStatus.SelectedItem!.ToString()),
                    new SqlParameter("@Description", (object?)txtDescription.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@FlatId", selectedFlatId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Data Updated Successfully!");
                ClearForm();
                LoadFlats();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating flat: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedFlatId <= 0)
            {
                UIHelper.ShowWarning("Please select a flat to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this flat?"))
            {
                return;
            }

            try
            {
                string deleteQuery = "DELETE FROM dbo.Flats WHERE FlatId = @FlatId";
                DbHelper.ExecuteNonQuery(deleteQuery, new[] { new SqlParameter("@FlatId", selectedFlatId) });

                UIHelper.ShowSuccess("Data Deleted Successfully!");
                ClearForm();
                LoadFlats();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Cannot delete flat. Active tenant, rent or complaint records exist.\nDetails: {ex.Message}");
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFlatNumber.Text) || cmbBuilding.SelectedValue == null)
            {
                UIHelper.ShowWarning("Please fill all required fields (Flat Number and Building).");
                return false;
            }

            if (!decimal.TryParse(txtMonthlyRent.Text.Trim(), out decimal rent) || rent < 0)
            {
                UIHelper.ShowWarning("Please enter a valid positive number for Monthly Rent.");
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            selectedFlatId = 0;
            txtFlatNumber.Clear();
            txtDescription.Clear();
            txtMonthlyRent.Text = "15000";
            numFloor.Value = 1;
            numBedrooms.Value = 2;
            numBathrooms.Value = 2;
            numAreaSqFt.Value = 1200;
            cmbStatus.SelectedIndex = 0;
            if (cmbBuilding.Items.Count > 0) cmbBuilding.SelectedIndex = 0;
        }
    }
}
