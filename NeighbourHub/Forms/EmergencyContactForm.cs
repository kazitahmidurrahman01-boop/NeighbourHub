using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class EmergencyContactForm : Form
    {
        private int selectedContactId = 0;
        private ComboBox cmbServiceName = null!;
        private TextBox txtContactPerson = null!;
        private TextBox txtPhone = null!;
        private TextBox txtAltPhone = null!;
        private TextBox txtAvailableHours = null!;
        private TextBox txtAddress = null!;
        private ComboBox cmbBuilding = null!;

        private TextBox txtSearch = null!;
        private DataGridView dgvContacts = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public EmergencyContactForm()
        {
            InitializeComponent();
            LoadBuildingsDropdown();
            LoadContacts();
        }

        private void InitializeComponent()
        {
            this.Text = "Emergency Directory & Rapid Contacts - NeighbourHub";
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
                Text = "🚨 Emergency Contacts, Security & Rapid Services Directory",
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
            inputPanel.Controls.Add(CreateLabel("Service / Department *:", 16, top));
            cmbServiceName = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbServiceName.Items.AddRange(new object[] {
                "Police Emergency", "Fire Service & Civil Defence", "Ambulance Service",
                "Building Security In-charge", "24/7 Electrician", "24/7 Plumber",
                "Building Manager", "Hospital / Doctor", "Gas Leak Emergency", "Lift Emergency Helpline"
            });
            cmbServiceName.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbServiceName);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Contact Person Name:", 16, top));
            txtContactPerson = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. Salam Hawlader" };
            inputPanel.Controls.Add(txtContactPerson);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Primary Phone Number *:", 16, top));
            txtPhone = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. 999 or 01700112233" };
            inputPanel.Controls.Add(txtPhone);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Alternate Phone / Landline:", 16, top));
            txtAltPhone = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtAltPhone);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Building (Optional):", 16, top));
            cmbBuilding = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbBuilding);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Available Hours:", 16, top));
            txtAvailableHours = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), Text = "24/7" };
            inputPanel.Controls.Add(txtAvailableHours);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Station / Office Address:", 16, top));
            txtAddress = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. Gulshan Thana, Dhaka" };
            inputPanel.Controls.Add(txtAddress);
            top += 58;

            // Buttons
            btnAdd = new Button { Text = "➕ Add Contact", Location = new Point(16, top), Size = new Size(140, 36) };
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

            var lblSearch = new Label { Text = "Search Contact:", Location = new Point(12, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(120, 11), Size = new Size(240, 28) };
            txtSearch.TextChanged += (s, e) => FilterContacts();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(375, 9), Size = new Size(75, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); LoadContacts(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(btnRefresh);

            dgvContacts = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvContacts);
            dgvContacts.CellClick += DgvContacts_CellClick;

            rightPanel.Controls.Add(dgvContacts);
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
                var emptyRow = dtBuildings.NewRow();
                emptyRow["BuildingId"] = DBNull.Value;
                emptyRow["BuildingName"] = "-- All Buildings (General) --";
                dtBuildings.Rows.InsertAt(emptyRow, 0);

                cmbBuilding.DataSource = dtBuildings;
                cmbBuilding.DisplayMember = "BuildingName";
                cmbBuilding.ValueMember = "BuildingId";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading buildings: {ex.Message}");
            }
        }

        private void LoadContacts()
        {
            try
            {
                string query = @"
                    SELECT c.ContactId, c.ServiceName, c.ContactPerson, c.Phone, c.AltPhone,
                           c.AvailableHours, c.Address, ISNULL(b.BuildingName, 'All Buildings') AS BuildingName,
                           c.BuildingId
                    FROM dbo.EmergencyContacts c
                    LEFT JOIN dbo.Buildings b ON c.BuildingId = b.BuildingId
                    ORDER BY c.ContactId ASC";

                var dt = DbHelper.ExecuteDataTable(query);
                dgvContacts.DataSource = dt;

                if (dgvContacts.Columns["ContactId"] != null) dgvContacts.Columns["ContactId"].Width = 60;
                if (dgvContacts.Columns["BuildingId"] != null) dgvContacts.Columns["BuildingId"].Visible = false;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load emergency contacts: {ex.Message}");
            }
        }

        private void FilterContacts()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string query = @"
                    SELECT c.ContactId, c.ServiceName, c.ContactPerson, c.Phone, c.AltPhone,
                           c.AvailableHours, c.Address, ISNULL(b.BuildingName, 'All Buildings') AS BuildingName,
                           c.BuildingId
                    FROM dbo.EmergencyContacts c
                    LEFT JOIN dbo.Buildings b ON c.BuildingId = b.BuildingId
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (c.ServiceName LIKE @Keyword OR c.ContactPerson LIKE @Keyword OR c.Phone LIKE @Keyword OR c.Address LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                query += " ORDER BY c.ContactId ASC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvContacts.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvContacts_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvContacts.Rows.Count)
            {
                var row = dgvContacts.Rows[e.RowIndex];
                selectedContactId = Convert.ToInt32(row.Cells["ContactId"].Value);
                txtContactPerson.Text = row.Cells["ContactPerson"].Value?.ToString() ?? string.Empty;
                txtPhone.Text = row.Cells["Phone"].Value?.ToString() ?? string.Empty;
                txtAltPhone.Text = row.Cells["AltPhone"].Value?.ToString() ?? string.Empty;
                txtAvailableHours.Text = row.Cells["AvailableHours"].Value?.ToString() ?? "24/7";
                txtAddress.Text = row.Cells["Address"].Value?.ToString() ?? string.Empty;

                cmbServiceName.SelectedItem = row.Cells["ServiceName"].Value?.ToString() ?? "Police Emergency";

                if (row.Cells["BuildingId"].Value != DBNull.Value)
                    cmbBuilding.SelectedValue = Convert.ToInt32(row.Cells["BuildingId"].Value);
                else
                    cmbBuilding.SelectedIndex = 0;
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                UIHelper.ShowWarning("Please enter a phone number.");
                return;
            }

            try
            {
                object buildingId = (cmbBuilding.SelectedValue == null || cmbBuilding.SelectedValue == DBNull.Value)
                    ? DBNull.Value
                    : cmbBuilding.SelectedValue;

                string insertQuery = @"
                    INSERT INTO dbo.EmergencyContacts (BuildingId, ServiceName, ContactPerson, Phone, AltPhone, AvailableHours, Address)
                    VALUES (@BuildingId, @ServiceName, @ContactPerson, @Phone, @AltPhone, @AvailableHours, @Address)";

                var parameters = new[]
                {
                    new SqlParameter("@BuildingId", buildingId),
                    new SqlParameter("@ServiceName", cmbServiceName.SelectedItem!.ToString()),
                    new SqlParameter("@ContactPerson", (object?)txtContactPerson.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Phone", txtPhone.Text.Trim()),
                    new SqlParameter("@AltPhone", (object?)txtAltPhone.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@AvailableHours", txtAvailableHours.Text.Trim()),
                    new SqlParameter("@Address", (object?)txtAddress.Text.Trim() ?? DBNull.Value)
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Emergency Contact Added Successfully!");
                ClearForm();
                LoadContacts();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error adding emergency contact: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedContactId <= 0)
            {
                UIHelper.ShowWarning("Please select a contact to update.");
                return;
            }

            try
            {
                object buildingId = (cmbBuilding.SelectedValue == null || cmbBuilding.SelectedValue == DBNull.Value)
                    ? DBNull.Value
                    : cmbBuilding.SelectedValue;

                string updateQuery = @"
                    UPDATE dbo.EmergencyContacts 
                    SET BuildingId = @BuildingId, ServiceName = @ServiceName, ContactPerson = @ContactPerson, 
                        Phone = @Phone, AltPhone = @AltPhone, AvailableHours = @AvailableHours, Address = @Address
                    WHERE ContactId = @ContactId";

                var parameters = new[]
                {
                    new SqlParameter("@BuildingId", buildingId),
                    new SqlParameter("@ServiceName", cmbServiceName.SelectedItem!.ToString()),
                    new SqlParameter("@ContactPerson", (object?)txtContactPerson.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Phone", txtPhone.Text.Trim()),
                    new SqlParameter("@AltPhone", (object?)txtAltPhone.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@AvailableHours", txtAvailableHours.Text.Trim()),
                    new SqlParameter("@Address", (object?)txtAddress.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@ContactId", selectedContactId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Emergency Contact Updated Successfully!");
                ClearForm();
                LoadContacts();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating emergency contact: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedContactId <= 0)
            {
                UIHelper.ShowWarning("Please select a contact to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this contact?"))
            {
                return;
            }

            try
            {
                DbHelper.ExecuteNonQuery("DELETE FROM dbo.EmergencyContacts WHERE ContactId = @ContactId",
                    new[] { new SqlParameter("@ContactId", selectedContactId) });

                UIHelper.ShowSuccess("Emergency Contact Deleted Successfully!");
                ClearForm();
                LoadContacts();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error deleting contact: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            selectedContactId = 0;
            txtContactPerson.Clear();
            txtPhone.Clear();
            txtAltPhone.Clear();
            txtAvailableHours.Text = "24/7";
            txtAddress.Clear();
            cmbServiceName.SelectedIndex = 0;
            if (cmbBuilding.Items.Count > 0) cmbBuilding.SelectedIndex = 0;
        }
    }
}
