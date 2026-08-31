using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class ResidentManagementForm : Form
    {
        private int selectedResidentId = 0;
        private ComboBox cmbUser = null!;
        private ComboBox cmbFlat = null!;
        private ComboBox cmbRelationship = null!;
        private TextBox txtNID = null!;
        private TextBox txtProfession = null!;
        private DateTimePicker dtpMoveIn = null!;
        private ComboBox cmbStatus = null!;

        private TextBox txtSearch = null!;
        private DataGridView dgvResidents = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public ResidentManagementForm()
        {
            InitializeComponent();
            LoadDropdowns();
            LoadResidents();
        }

        private void InitializeComponent()
        {
            this.Text = "Resident Directory & Management - NeighbourHub";
            this.Size = new Size(1040, 660);
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
                Text = "👨‍👩‍👧‍👦 Resident & Tenant Directory",
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
                Padding = new Padding(16),
                AutoScroll = true
            };

            int top = 16;
            inputPanel.Controls.Add(CreateLabel("Resident User *:", 16, top));
            cmbUser = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbUser);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Residing Flat *:", 16, top));
            cmbFlat = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbFlat);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Relationship to Tenant *:", 16, top));
            cmbRelationship = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRelationship.Items.AddRange(new object[] { "Self", "Spouse", "Child", "Parent", "Sibling", "Other" });
            cmbRelationship.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbRelationship);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("National ID (NID) / Passport:", 16, top));
            txtNID = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. 1995269485123456" };
            inputPanel.Controls.Add(txtNID);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Profession / Occupation:", 16, top));
            txtProfession = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. Software Engineer" };
            inputPanel.Controls.Add(txtProfession);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Move-In Date:", 16, top));
            dtpMoveIn = new DateTimePicker { Location = new Point(16, top + 22), Size = new Size(300, 28), Format = DateTimePickerFormat.Short };
            inputPanel.Controls.Add(dtpMoveIn);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Status *:", 16, top));
            cmbStatus = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            cmbStatus.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbStatus);
            top += 58;

            // Buttons
            btnAdd = new Button { Text = "➕ Add Resident", Location = new Point(16, top), Size = new Size(140, 36) };
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

            var lblSearch = new Label { Text = "Search Resident:", Location = new Point(12, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(125, 11), Size = new Size(240, 28) };
            txtSearch.TextChanged += (s, e) => FilterResidents();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(380, 9), Size = new Size(80, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); LoadResidents(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(btnRefresh);

            dgvResidents = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvResidents);
            dgvResidents.CellClick += DgvResidents_CellClick;

            rightPanel.Controls.Add(dgvResidents);
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
                var dtUsers = DbHelper.ExecuteDataTable("SELECT UserId, FullName + ' (' + Phone + ')' AS UserDisplay FROM dbo.Users WHERE Role = 'Resident' AND Status = 'Active'");
                cmbUser.DataSource = dtUsers;
                cmbUser.DisplayMember = "UserDisplay";
                cmbUser.ValueMember = "UserId";

                string flatQuery = @"
                    SELECT f.FlatId, 'Flat ' + f.FlatNumber + ' - ' + b.BuildingName AS FlatDisplay
                    FROM dbo.Flats f
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId";

                var dtFlats = DbHelper.ExecuteDataTable(flatQuery);
                cmbFlat.DataSource = dtFlats;
                cmbFlat.DisplayMember = "FlatDisplay";
                cmbFlat.ValueMember = "FlatId";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading dropdowns: {ex.Message}");
            }
        }

        private void LoadResidents()
        {
            try
            {
                string query = @"
                    SELECT r.ResidentId, u.FullName AS ResidentName, u.Phone, u.Email,
                           f.FlatNumber, b.BuildingName, r.Relationship, r.Profession,
                           r.NID, r.MoveInDate, r.Status, r.UserId, r.FlatId
                    FROM dbo.Residents r
                    INNER JOIN dbo.Users u ON r.UserId = u.UserId
                    INNER JOIN dbo.Flats f ON r.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    ORDER BY r.ResidentId DESC";

                var dt = DbHelper.ExecuteDataTable(query);
                dgvResidents.DataSource = dt;

                if (dgvResidents.Columns["ResidentId"] != null) dgvResidents.Columns["ResidentId"].Width = 60;
                if (dgvResidents.Columns["UserId"] != null) dgvResidents.Columns["UserId"].Visible = false;
                if (dgvResidents.Columns["FlatId"] != null) dgvResidents.Columns["FlatId"].Visible = false;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load residents: {ex.Message}");
            }
        }

        private void FilterResidents()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string query = @"
                    SELECT r.ResidentId, u.FullName AS ResidentName, u.Phone, u.Email,
                           f.FlatNumber, b.BuildingName, r.Relationship, r.Profession,
                           r.NID, r.MoveInDate, r.Status, r.UserId, r.FlatId
                    FROM dbo.Residents r
                    INNER JOIN dbo.Users u ON r.UserId = u.UserId
                    INNER JOIN dbo.Flats f ON r.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (u.FullName LIKE @Keyword OR u.Phone LIKE @Keyword OR f.FlatNumber LIKE @Keyword OR b.BuildingName LIKE @Keyword OR r.Profession LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                query += " ORDER BY r.ResidentId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvResidents.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvResidents_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvResidents.Rows.Count)
            {
                var row = dgvResidents.Rows[e.RowIndex];
                selectedResidentId = Convert.ToInt32(row.Cells["ResidentId"].Value);
                txtNID.Text = row.Cells["NID"].Value?.ToString() ?? string.Empty;
                txtProfession.Text = row.Cells["Profession"].Value?.ToString() ?? string.Empty;

                if (row.Cells["MoveInDate"].Value != DBNull.Value)
                    dtpMoveIn.Value = Convert.ToDateTime(row.Cells["MoveInDate"].Value);

                if (row.Cells["UserId"].Value != DBNull.Value)
                    cmbUser.SelectedValue = Convert.ToInt32(row.Cells["UserId"].Value);

                if (row.Cells["FlatId"].Value != DBNull.Value)
                    cmbFlat.SelectedValue = Convert.ToInt32(row.Cells["FlatId"].Value);

                cmbRelationship.SelectedItem = row.Cells["Relationship"].Value?.ToString() ?? "Self";
                cmbStatus.SelectedItem = row.Cells["Status"].Value?.ToString() ?? "Active";
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (cmbUser.SelectedValue == null || cmbFlat.SelectedValue == null)
            {
                UIHelper.ShowWarning("Please select both a Resident User and a Flat.");
                return;
            }

            try
            {
                string insertQuery = @"
                    INSERT INTO dbo.Residents (UserId, FlatId, Relationship, NID, Profession, MoveInDate, Status)
                    VALUES (@UserId, @FlatId, @Relationship, @NID, @Profession, @MoveInDate, @Status)";

                var parameters = new[]
                {
                    new SqlParameter("@UserId", Convert.ToInt32(cmbUser.SelectedValue)),
                    new SqlParameter("@FlatId", Convert.ToInt32(cmbFlat.SelectedValue)),
                    new SqlParameter("@Relationship", cmbRelationship.SelectedItem!.ToString()),
                    new SqlParameter("@NID", (object?)txtNID.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Profession", (object?)txtProfession.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@MoveInDate", dtpMoveIn.Value.Date),
                    new SqlParameter("@Status", cmbStatus.SelectedItem!.ToString())
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Data Saved Successfully!");
                ClearForm();
                LoadResidents();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error adding resident: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedResidentId <= 0)
            {
                UIHelper.ShowWarning("Please select a resident from the table to update.");
                return;
            }

            try
            {
                string updateQuery = @"
                    UPDATE dbo.Residents 
                    SET UserId = @UserId, FlatId = @FlatId, Relationship = @Relationship, 
                        NID = @NID, Profession = @Profession, MoveInDate = @MoveInDate, Status = @Status
                    WHERE ResidentId = @ResidentId";

                var parameters = new[]
                {
                    new SqlParameter("@UserId", Convert.ToInt32(cmbUser.SelectedValue)),
                    new SqlParameter("@FlatId", Convert.ToInt32(cmbFlat.SelectedValue)),
                    new SqlParameter("@Relationship", cmbRelationship.SelectedItem!.ToString()),
                    new SqlParameter("@NID", (object?)txtNID.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@Profession", (object?)txtProfession.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@MoveInDate", dtpMoveIn.Value.Date),
                    new SqlParameter("@Status", cmbStatus.SelectedItem!.ToString()),
                    new SqlParameter("@ResidentId", selectedResidentId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Data Updated Successfully!");
                ClearForm();
                LoadResidents();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating resident: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedResidentId <= 0)
            {
                UIHelper.ShowWarning("Please select a resident to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this resident entry?"))
            {
                return;
            }

            try
            {
                DbHelper.ExecuteNonQuery("DELETE FROM dbo.Residents WHERE ResidentId = @ResidentId",
                    new[] { new SqlParameter("@ResidentId", selectedResidentId) });

                UIHelper.ShowSuccess("Data Deleted Successfully!");
                ClearForm();
                LoadResidents();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error deleting resident: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            selectedResidentId = 0;
            txtNID.Clear();
            txtProfession.Clear();
            dtpMoveIn.Value = DateTime.Today;
            cmbRelationship.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
        }
    }
}
