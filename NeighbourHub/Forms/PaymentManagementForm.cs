using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class PaymentManagementForm : Form
    {
        private int selectedPaymentId = 0;
        private ComboBox cmbRentBill = null!;
        private TextBox txtAmountPaid = null!;
        private DateTimePicker dtpPaymentDate = null!;
        private ComboBox cmbPaymentMethod = null!;
        private TextBox txtTransactionRef = null!;
        private TextBox txtPaidBy = null!;
        private TextBox txtRemarks = null!;

        private TextBox txtSearch = null!;
        private ComboBox cmbMethodFilter = null!;
        private DataGridView dgvPayments = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;
        private Label lblTotalPaidSummary = null!;

        public PaymentManagementForm()
        {
            InitializeComponent();
            LoadRentBillsDropdown();
            LoadPayments();
        }

        private void InitializeComponent()
        {
            this.Text = "Payment Processing & Receipts - NeighbourHub";
            this.Size = new Size(1080, 700);
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
                Text = "💳 Payment Collection & Receipt Processing",
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
            inputPanel.Controls.Add(CreateLabel("Select Rent Bill / Invoice *:", 16, top));
            cmbRentBill = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRentBill.SelectedIndexChanged += CmbRentBill_SelectedIndexChanged;
            inputPanel.Controls.Add(cmbRentBill);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Amount Paid (৳ BDT) *:", 16, top));
            txtAmountPaid = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtAmountPaid);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Payment Date *:", 16, top));
            dtpPaymentDate = new DateTimePicker { Location = new Point(16, top + 22), Size = new Size(300, 28), Format = DateTimePickerFormat.Short };
            inputPanel.Controls.Add(dtpPaymentDate);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Payment Method *:", 16, top));
            cmbPaymentMethod = new ComboBox { Location = new Point(16, top + 22), Size = new Size(300, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPaymentMethod.Items.AddRange(new object[] { "bKash/Nagad", "Bank Transfer", "Cash", "Card / POS", "Cheque" });
            cmbPaymentMethod.SelectedIndex = 0;
            inputPanel.Controls.Add(cmbPaymentMethod);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Transaction / Receipt Ref:", 16, top));
            txtTransactionRef = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. BK-9948271, TXN-5582" };
            inputPanel.Controls.Add(txtTransactionRef);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Paid By (Payer Name) *:", 16, top));
            txtPaidBy = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28) };
            inputPanel.Controls.Add(txtPaidBy);
            top += 54;

            inputPanel.Controls.Add(CreateLabel("Remarks / Notes:", 16, top));
            txtRemarks = new TextBox { Location = new Point(16, top + 22), Size = new Size(300, 28), PlaceholderText = "e.g. Rent + Utilities" };
            inputPanel.Controls.Add(txtRemarks);
            top += 58;

            // Buttons
            btnAdd = new Button { Text = "➕ Record Payment", Location = new Point(16, top), Size = new Size(140, 36) };
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
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblSearch = new Label { Text = "Search Payment:", Location = new Point(12, 12), AutoSize = true, Font = UIHelper.BodyBoldFont };
            txtSearch = new TextBox { Location = new Point(125, 9), Size = new Size(180, 28) };
            txtSearch.TextChanged += (s, e) => FilterPayments();

            var lblFilter = new Label { Text = "Method:", Location = new Point(315, 12), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbMethodFilter = new ComboBox { Location = new Point(375, 9), Size = new Size(130, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMethodFilter.Items.AddRange(new object[] { "All Methods", "bKash/Nagad", "Bank Transfer", "Cash", "Card / POS" });
            cmbMethodFilter.SelectedIndex = 0;
            cmbMethodFilter.SelectedIndexChanged += (s, e) => FilterPayments();

            var btnRefresh = new Button { Text = "Refresh", Location = new Point(515, 7), Size = new Size(75, 30) };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); cmbMethodFilter.SelectedIndex = 0; LoadPayments(); };

            lblTotalPaidSummary = new Label
            {
                Text = "Total Payments Collected: ৳0",
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.Success,
                Location = new Point(12, 42),
                AutoSize = true
            };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblFilter);
            searchPanel.Controls.Add(cmbMethodFilter);
            searchPanel.Controls.Add(btnRefresh);
            searchPanel.Controls.Add(lblTotalPaidSummary);

            dgvPayments = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvPayments);
            dgvPayments.CellClick += DgvPayments_CellClick;

            rightPanel.Controls.Add(dgvPayments);
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

        private void LoadRentBillsDropdown()
        {
            try
            {
                string query = @"
                    SELECT r.RentId, 
                           u.FullName + ' - Flat ' + f.FlatNumber + ' (' + r.Month + ' ' + CAST(r.Year AS NVARCHAR(4)) + ' - Total ৳' + CAST(CAST(r.RentAmount + r.UtilityCharges AS INT) AS NVARCHAR(20)) + ' [' + r.Status + '])' AS BillDisplay,
                           (r.RentAmount + r.UtilityCharges) AS TotalAmount,
                           u.FullName, r.TenantId, r.FlatId
                    FROM dbo.Rent r
                    INNER JOIN dbo.Tenants t ON r.TenantId = t.TenantId
                    INNER JOIN dbo.Users u ON t.UserId = u.UserId
                    INNER JOIN dbo.Flats f ON r.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId";

                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    query += $" WHERE p.OwnerUserId = {SessionManager.CurrentUser.UserId}";
                }
                else if (SessionManager.CurrentUser?.Role == "Resident")
                {
                    query += $" WHERE t.UserId = {SessionManager.CurrentUser.UserId}";
                }

                query += " ORDER BY r.RentId DESC";

                var dt = DbHelper.ExecuteDataTable(query);
                cmbRentBill.DataSource = dt;
                cmbRentBill.DisplayMember = "BillDisplay";
                cmbRentBill.ValueMember = "RentId";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading rent bills: {ex.Message}");
            }
        }

        private void CmbRentBill_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbRentBill.SelectedItem is DataRowView drv)
            {
                if (drv.Row.Table.Columns.Contains("TotalAmount"))
                {
                    txtAmountPaid.Text = drv["TotalAmount"].ToString();
                }
                if (drv.Row.Table.Columns.Contains("FullName"))
                {
                    txtPaidBy.Text = drv["FullName"].ToString();
                }
            }
        }

        private void LoadPayments()
        {
            try
            {
                string query = @"
                    SELECT p.PaymentId, u.FullName AS TenantName, f.FlatNumber, b.BuildingName,
                           p.AmountPaid, p.PaymentDate, p.PaymentMethod, p.TransactionRef,
                           p.PaidBy, p.Status, p.Remarks, p.RentId, p.TenantId, p.FlatId
                    FROM dbo.Payments p
                    INNER JOIN dbo.Tenants t ON p.TenantId = t.TenantId
                    INNER JOIN dbo.Users u ON t.UserId = u.UserId
                    INNER JOIN dbo.Flats f ON p.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties pr ON b.PropertyId = pr.PropertyId ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();
                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    query += " WHERE pr.OwnerUserId = @OwnerId ";
                    parameters.Add(new SqlParameter("@OwnerId", SessionManager.CurrentUser.UserId));
                }
                else if (SessionManager.CurrentUser?.Role == "Resident")
                {
                    query += " WHERE t.UserId = @UserId ";
                    parameters.Add(new SqlParameter("@UserId", SessionManager.CurrentUser.UserId));
                }

                query += " ORDER BY p.PaymentId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvPayments.DataSource = dt;

                if (dgvPayments.Columns["PaymentId"] != null) dgvPayments.Columns["PaymentId"].Width = 60;
                if (dgvPayments.Columns["RentId"] != null) dgvPayments.Columns["RentId"].Visible = false;
                if (dgvPayments.Columns["TenantId"] != null) dgvPayments.Columns["TenantId"].Visible = false;
                if (dgvPayments.Columns["FlatId"] != null) dgvPayments.Columns["FlatId"].Visible = false;

                decimal sum = 0;
                foreach (DataRow row in dt.Rows)
                {
                    sum += Convert.ToDecimal(row["AmountPaid"]);
                }
                lblTotalPaidSummary.Text = $"💵 Total Payments Collected: ৳{sum:N0}";
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Failed to load payments: {ex.Message}");
            }
        }

        private void FilterPayments()
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string selectedMethod = cmbMethodFilter.SelectedItem?.ToString() ?? "All Methods";

                string query = @"
                    SELECT p.PaymentId, u.FullName AS TenantName, f.FlatNumber, b.BuildingName,
                           p.AmountPaid, p.PaymentDate, p.PaymentMethod, p.TransactionRef,
                           p.PaidBy, p.Status, p.Remarks, p.RentId, p.TenantId, p.FlatId
                    FROM dbo.Payments p
                    INNER JOIN dbo.Tenants t ON p.TenantId = t.TenantId
                    INNER JOIN dbo.Users u ON t.UserId = u.UserId
                    INNER JOIN dbo.Flats f ON p.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties pr ON b.PropertyId = pr.PropertyId
                    WHERE 1=1 ";

                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (SessionManager.CurrentUser?.Role == "Property Owner")
                {
                    query += " AND pr.OwnerUserId = @OwnerId ";
                    parameters.Add(new SqlParameter("@OwnerId", SessionManager.CurrentUser.UserId));
                }
                else if (SessionManager.CurrentUser?.Role == "Resident")
                {
                    query += " WHERE t.UserId = @UserId ";
                    parameters.Add(new SqlParameter("@UserId", SessionManager.CurrentUser.UserId));
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    query += " AND (u.FullName LIKE @Keyword OR p.PaidBy LIKE @Keyword OR p.TransactionRef LIKE @Keyword OR f.FlatNumber LIKE @Keyword) ";
                    parameters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
                }

                if (selectedMethod != "All Methods")
                {
                    query += " AND p.PaymentMethod = @Method ";
                    parameters.Add(new SqlParameter("@Method", selectedMethod));
                }

                query += " ORDER BY p.PaymentId DESC";

                var dt = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvPayments.DataSource = dt;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Filter failed: {ex.Message}");
            }
        }

        private void DgvPayments_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvPayments.Rows.Count)
            {
                var row = dgvPayments.Rows[e.RowIndex];
                selectedPaymentId = Convert.ToInt32(row.Cells["PaymentId"].Value);
                txtAmountPaid.Text = row.Cells["AmountPaid"].Value?.ToString() ?? "0";
                txtPaidBy.Text = row.Cells["PaidBy"].Value?.ToString() ?? string.Empty;
                txtTransactionRef.Text = row.Cells["TransactionRef"].Value?.ToString() ?? string.Empty;
                txtRemarks.Text = row.Cells["Remarks"].Value?.ToString() ?? string.Empty;

                if (row.Cells["PaymentDate"].Value != DBNull.Value)
                    dtpPaymentDate.Value = Convert.ToDateTime(row.Cells["PaymentDate"].Value);

                if (row.Cells["RentId"].Value != DBNull.Value)
                    cmbRentBill.SelectedValue = Convert.ToInt32(row.Cells["RentId"].Value);

                cmbPaymentMethod.SelectedItem = row.Cells["PaymentMethod"].Value?.ToString() ?? "Cash";
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                int rentId = Convert.ToInt32(cmbRentBill.SelectedValue);

                // Fetch TenantId and FlatId from Rent record
                var dtRent = DbHelper.ExecuteDataTable("SELECT TenantId, FlatId FROM dbo.Rent WHERE RentId = @RentId",
                    new[] { new SqlParameter("@RentId", rentId) });

                if (dtRent.Rows.Count == 0)
                {
                    UIHelper.ShowWarning("Selected rent bill could not be found.");
                    return;
                }

                int tenantId = Convert.ToInt32(dtRent.Rows[0]["TenantId"]);
                int flatId = Convert.ToInt32(dtRent.Rows[0]["FlatId"]);
                decimal amount = decimal.Parse(txtAmountPaid.Text.Trim());

                string insertQuery = @"
                    INSERT INTO dbo.Payments (RentId, TenantId, FlatId, AmountPaid, PaymentDate, PaymentMethod, TransactionRef, PaidBy, Status, Remarks)
                    VALUES (@RentId, @TenantId, @FlatId, @AmountPaid, @PaymentDate, @PaymentMethod, @TransactionRef, @PaidBy, 'Completed', @Remarks)";

                var parameters = new[]
                {
                    new SqlParameter("@RentId", rentId),
                    new SqlParameter("@TenantId", tenantId),
                    new SqlParameter("@FlatId", flatId),
                    new SqlParameter("@AmountPaid", amount),
                    new SqlParameter("@PaymentDate", dtpPaymentDate.Value),
                    new SqlParameter("@PaymentMethod", cmbPaymentMethod.SelectedItem!.ToString()),
                    new SqlParameter("@TransactionRef", (object?)txtTransactionRef.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@PaidBy", txtPaidBy.Text.Trim()),
                    new SqlParameter("@Remarks", (object?)txtRemarks.Text.Trim() ?? DBNull.Value)
                };

                DbHelper.ExecuteNonQuery(insertQuery, parameters);

                // Mark rent as Paid
                DbHelper.ExecuteNonQuery("UPDATE dbo.Rent SET Status = 'Paid' WHERE RentId = @RentId",
                    new[] { new SqlParameter("@RentId", rentId) });

                UIHelper.ShowSuccess("Payment Recorded Successfully & Rent Bill Marked as Paid!");
                ClearForm();
                LoadRentBillsDropdown();
                LoadPayments();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error recording payment: {ex.Message}");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (selectedPaymentId <= 0)
            {
                UIHelper.ShowWarning("Please select a payment record from the table to update.");
                return;
            }

            if (!ValidateInputs()) return;

            try
            {
                string updateQuery = @"
                    UPDATE dbo.Payments 
                    SET AmountPaid = @AmountPaid, PaymentDate = @PaymentDate, 
                        PaymentMethod = @PaymentMethod, TransactionRef = @TransactionRef, 
                        PaidBy = @PaidBy, Remarks = @Remarks
                    WHERE PaymentId = @PaymentId";

                var parameters = new[]
                {
                    new SqlParameter("@AmountPaid", decimal.Parse(txtAmountPaid.Text.Trim())),
                    new SqlParameter("@PaymentDate", dtpPaymentDate.Value),
                    new SqlParameter("@PaymentMethod", cmbPaymentMethod.SelectedItem!.ToString()),
                    new SqlParameter("@TransactionRef", (object?)txtTransactionRef.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@PaidBy", txtPaidBy.Text.Trim()),
                    new SqlParameter("@Remarks", (object?)txtRemarks.Text.Trim() ?? DBNull.Value),
                    new SqlParameter("@PaymentId", selectedPaymentId)
                };

                DbHelper.ExecuteNonQuery(updateQuery, parameters);
                UIHelper.ShowSuccess("Payment Record Updated Successfully!");
                ClearForm();
                LoadPayments();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error updating payment: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (selectedPaymentId <= 0)
            {
                UIHelper.ShowWarning("Please select a payment record to delete.");
                return;
            }

            if (!UIHelper.Confirm("Are you sure you want to delete this payment record?"))
            {
                return;
            }

            try
            {
                DbHelper.ExecuteNonQuery("DELETE FROM dbo.Payments WHERE PaymentId = @PaymentId",
                    new[] { new SqlParameter("@PaymentId", selectedPaymentId) });

                UIHelper.ShowSuccess("Payment Record Deleted Successfully!");
                ClearForm();
                LoadRentBillsDropdown();
                LoadPayments();
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error deleting payment: {ex.Message}");
            }
        }

        private bool ValidateInputs()
        {
            if (cmbRentBill.SelectedValue == null)
            {
                UIHelper.ShowWarning("Please select a Rent Bill.");
                return false;
            }

            if (!decimal.TryParse(txtAmountPaid.Text.Trim(), out decimal amt) || amt <= 0)
            {
                UIHelper.ShowWarning("Please enter a valid positive Amount Paid.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPaidBy.Text))
            {
                UIHelper.ShowWarning("Please enter the payer name (Paid By).");
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            selectedPaymentId = 0;
            txtAmountPaid.Clear();
            txtTransactionRef.Clear();
            txtPaidBy.Clear();
            txtRemarks.Clear();
            dtpPaymentDate.Value = DateTime.Now;
            cmbPaymentMethod.SelectedIndex = 0;
        }
    }
}
