using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class UtilityManagementForm : Form
    {
        private int selectedUtilityId = 0;
        private ComboBox cmbBuilding = null!;
        private ComboBox cmbFlat = null!;
        private ComboBox cmbUtilityType = null!;
        private ComboBox cmbBillingMonth = null!;
        private NumericUpDown numBillingYear = null!;
        private TextBox txtAmount = null!;
        private DateTimePicker dtpDueDate = null!;
        private ComboBox cmbStatus = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbTypeFilter = null!;
        private DataGridView dgvUtilities = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;

        public UtilityManagementForm()
        {
            InitializeComponent();
            LoadDropdowns();
            LoadUtilities();
        }

        private void InitializeComponent()
        {
            this.Text = "Utility Bills & Metering - NeighbourHub";
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
                Text = "⚡ Utility Management (Electricity, Water, Gas, Generator)",
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
            inputPanel.Controls.Add(CreateLabel("Building *:", 16, top));
            cmbBuilding = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbBuilding);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Flat (Optional - Leave blank for Common Area):", 16, top));
            cmbFlat = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            inputPanel.Controls.Add(cmbFlat);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Utility Type *:", 16, top));
            cmbUtilityType = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUtilityType.Items.AddRange(new object[] { "Electricity", "Water", "Gas", "Generator Fuel", "Internet", "Waste Management" });
            cmbUtilityType.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbUtilityType);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Billing Month & Year *:", 16, top));
            cmbBillingMonth = new ComboBox { Location = new Point(16, top + 22), Size = new Size(160, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbBillingMonth.Items.AddRange(new object[] {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            });
            cmbBillingMonth.SelectedItem = DateTime.Now.ToString("MMMM");

            numBillingYear = new NumericUpDown { Location = new Point(186, top + 22), Size = new Size(130, 28), Minimum = 2020, Maximum = 2050, Value = DateTime.Now.Year };
            inputPanel.Controls.Add(cmbBillingMonth);
            inputPanel.Controls.Add(numBillingYear);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Bill Amount (৳ BDT) *:", 16, top));
            txtAmount = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), Text = "2500" };
            inputPanel.Controls.Add(txtAmount);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Due Date *:", 16, top));
            dtpDueDate = new DateTimePicker { Location = new Point(16, top + 22), Size = new Size(300, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(15) };
            inputPanel.Controls.Add(dtpDueDate);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Status *:", 16, top));
            cmbStatus = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new object[] { "Unpaid", "Paid" });
            cmbStatus.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbStatus);
            top += 58;

            // Buttons
            btnAdd = new Button { Text = "➕ Add Bill", Location = new Point(16, top), Size = new Size(140, 36) };
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
            txtSearch = new TextBox { Location = new Point(65, 11), Size = new Size(180, 28) };
            txtSearch.TextChanged += (s, e) => FilterUtilities();

            var lblFilterType = new Label { Text = "Type:", Location = new Point(255, 14), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbTypeFilter = new ComboBox { Location = new Point(300, 11), Size = new Size(140, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTypeFilter.Items.AddRange(new object[] { "All Types", "Electricity", "Water", "Gas", "Generator Fuel", "Internet", "Waste Management" });
            cmbTypeFilter.SelectedIndex = 0;
            cmbTypeFilter.SelectedIndexChanged += (s, e) => FilterUtilities();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(450, 9), Size = new Size(75, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbTypeFilter.SelectedIndex = 0; LoadUtilities(); };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblFilterType);
            searchPanel.Controls.Add(cmbTypeFilter);
            searchPanel.Controls.Add(btnRefresh);

            dgvUtilities = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvUtilities);
            dgvUtilities.CellClick += DgvUtilities_CellClick;

            rightPanel.Controls.Add(dgvUtilities);
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

                var dtFlats = DbHelper.ExecuteDataTable("SELECT FlatId, FlatNumber FROM dbo.Flats");
                var emptyRow = dtFlats.NewRow();
                emptyRow["FlatId"] = DBNull.Value;
                emptyRow["FlatNumber"] = "-- Common Area --";
                dtFlats.Rows.InsertAt(emptyRow, 0);

                cmbFlat.DataSource = dtFlats;
                cmbFlat.DisplayMember = "FlatNumber";
                cmbFlat.ValueMember = "FlatId";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading dropdowns: {ex.Message}");
            }
        }

        private void LoadUtilities()
        {
            try
            {
                string query = @"
                    SELECT u.UtilityId, b.BuildingName, ISNULL(f.FlatNumber, 'Common Area') AS FlatNumber,
                           u.UtilityType, u.BillingMonth, u.BillingYear, u.Amount, u.DueDate, u.Status,
                           u.BuildingId, u.FlatId
                    FROM dbo.Utilities u
                    INNER JOIN dbo.Buildings b ON u.BuildingId = b.BuildingId
                    LEFT JOIN dbo.Flats f ON u.FlatId = f.FlatId
                    ORDER BY u.UtilityId DESC";

                var dt = DbHelper.ExecuteDataTable(query);
                dgvUtilities.DataSource = dt;

                if (dgvUtilities.Columns["UtilityId"] != null) dgvUtilities.Columns["UtilityId"].Width = 60;
                if (dgvUtilities.Columns["BuildingId"] != null) dgvUtilities.Columns["BuildingId"].Visible = false;
                if (dgvUtilities.Columns["FlatId"] != null) dgvUtilities.Columns["FlatId"].Visible = false;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load utilities: {ex.Message}");
            }
        }

        private void FilterUtilities()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string selectedType = cmbTypeFilter.SelectedItem?.ToString() ?? "All Types";

                string query = @"
                    SELECT u.UtilityId, b.BuildingName, ISNULL(f.FlatNumber, 'Common Area') AS FlatNumber,
                           u.UtilityType, u.BillingMonth, u.BillingYear, u.Amount, u.DueDate, u.Status,
                           u.BuildingId, u.FlatId
                    FROM dbo.Utilities u
                    INNER JOIN dbo.Buildings b ON u.BuildingId = b.BuildingId
                    LEFT JOIN dbo.Flats f ON u.FlatId = f.FlatId
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (b.BuildingName LIKE @Keyword OR f.FlatNumber LIKE @Keyword OR u.BillingMonth LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                if (selectedType != "All Types")
                {
                    query += " AND u.UtilityType = @Type ";
                    parameters.Add(new SqlParameter("@Type", selectedType));
                }

                query += " ORDER BY u.UtilityId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvUtilities.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvUtilities_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvUtilities.Rows.Count)
            {
                var row = dgvUtilities.Rows[e.RowIndex];
                selectedUtilityId = Convert.ToInt32(row.Cells["UtilityId"].Value);
                txtAmount.Text = row.Cells["Amount"].Value?.ToString() ?? "0";

                cmbUtilityType.SelectedItem = row.Cells["UtilityType"].Value?.ToString() ?? "Electricity";
                cmbBillingMonth.SelectedItem = row.Cells["BillingMonth"].Value?.ToString() ?? "August";
                numBillingYear.Value = Convert.ToDecimal(row.Cells["BillingYear"].Value ?? DateTime.Now.Year);

                if (row.Cells["DueDate"].Value != DBNull.Value)
                    dtpDueDate.Value = Convert.ToDateTime(row.Cells["DueDate"].Value);

                if (row.Cells["BuildingId"].Value != DBNull.Value)
                    cmbBuilding.SelectedValue = Convert.ToInt32(row.Cells["BuildingId"].Value);

                if (row.Cells["FlatId"].Value != DBNull.Value)
                    cmbFlat.SelectedValue = Convert.ToInt32(row.Cells["FlatId"].Value);
                else
                    cmbFlat.SelectedIndex = 0;

                cmbStatus.SelectedItem = row.Cells["Status"].Value?.ToString() ?? "Unpaid";
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (cmbBuilding.SelectedValue == null)
            {
                UIHelper.ShowWarning("Please select a Building.");
                return;
            }

            if (!decimal.TryParse(txtAmount.Text.Trim(), out decimal amt) || amt <= 0)
            {
                UIHelper.ShowWarning("Please enter a valid positive bill amount.");
                return;
            }

            try
            {
                object flatId = (cmbFlat.SelectedValue == null || cmbFlat.SelectedValue == DBNull.Value)
                    ? DBNull.Value
                    : cmbFlat.SelectedValue;

                string insertQuery = @"
                    INSERT INTO dbo.Utilities (BuildingId, FlatId, UtilityType, BillingMonth, BillingYear, Amount, DueDate, Status)
                    VALUES (@BuildingId, @FlatId, @UtilityType, @BillingMonth, @BillingYear, @Amount, @DueDate, @Status)";

                var parameters = new[]
                {
                    new SqlParameter("@BuildingId", Convert.ToInt32(cmbBuilding.SelectedValue)),
                    new SqlParameter("@FlatId", flatId),
                    new SqlParameter("@UtilityType", cmbUtilityType.SelectedItem!.ToString()),
                    new SqlParameter("@BillingMonth", cmbBillingMonth.SelectedItem!.ToString()),
                    new SqlParameter("@BillingYear", (int)numBillingYear.Value),
                    new SqlParameter("@Amount", amt),
                    new SqlParameter("@DueDate", dtpDueDate.Value.Date),
                    new SqlParameter("@Status", cmbStatus.SelectedItem!.ToString())
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);
                UIHelper.ShowSuccess("Utility Bill Recorded Successfully!");
                ClearForm();
                LoadUtilities();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error recording utility bill: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedUtilityId <= 0)
            {
                UIHelper.ShowWarning("Please select a utility record from the table to update.");
                return;
            }

            if (!decimal.TryParse(txtAmount.Text.Trim(), out decimal amt) || amt <= 0)
            {
                UIHelper.ShowWarning("Please enter a valid positive bill amount.");
                return;
            }

            try
            {
                object flatId = (cmbFlat.SelectedValue == null || cmbFlat.SelectedValue == DBNull.Value)
                    ? DBNull.Value
                    : cmbFlat.SelectedValue;

                string updateQuery = @"
                    UPDATE dbo.Utilities 
                    SET BuildingId = @BuildingId, FlatId = @FlatId, UtilityType = @UtilityType, 
                        BillingMonth = @BillingMonth, BillingYear = @BillingYear, Amount = @Amount, 
                        DueDate = @DueDate, Status = @Status
                    WHERE UtilityId = @UtilityId";

                var parameters = new[]
                {
                    new SqlParameter("@BuildingId", Convert.ToInt32(cmbBuilding.SelectedValue)),
                    new SqlParameter("@FlatId", flatId),
                    new SqlParameter("@UtilityType", cmbUtilityType.SelectedItem!.ToString()),
                    new SqlParameter("@BillingMonth", cmbBillingMonth.SelectedItem!.ToString()),
                    new SqlParameter("@BillingYear", (int)numBillingYear.Value),
                    new SqlParameter("@Amount", amt),
                    new SqlParameter("@DueDate", dtpDueDate.Value.Date),
                    new SqlParameter("@Status", cmbStatus.SelectedItem!.ToString()),
                    new SqlParameter("@UtilityId", selectedUtilityId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Utility Bill Updated Successfully!");
                ClearForm();
                LoadUtilities();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating utility record: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedUtilityId <= 0)
            {
                UIHelper.ShowWarning("Please select a utility record to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this utility record?"))
            {
                return;
            }

            try
            {
                DbHelper.ExecuteNonQuery("DELETE FROM dbo.Utilities WHERE UtilityId = @UtilityId",
                    new[] { new SqlParameter("@UtilityId", selectedUtilityId) });

                UIHelper.ShowSuccess("Utility Record Deleted Successfully!");
                ClearForm();
                LoadUtilities();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error deleting utility bill: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            selectedUtilityId = 0;
            txtAmount.Text = "2500";
            dtpDueDate.Value = DateTime.Today.AddDays(15);
            cmbUtilityType.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            if (cmbFlat.Items.Count > 0) cmbFlat.SelectedIndex = 0;
        }
    }
}
