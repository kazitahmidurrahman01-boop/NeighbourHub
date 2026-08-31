using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class ReportsForm : Form
    {
        private ComboBox cmbReportType = null!;
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private Button btnGenerate = null!;
        private Button btnExportCsv = null!;
        private Label lblReportStats = null!;
        private DataGridView dgvReportResults = null!;
        private DataTable? currentReportData;

        public ReportsForm()
        {
            InitializeComponent();
            PopulateReportTypes();
        }

        private void InitializeComponent()
        {
            this.Text = "Analytics & Reports Generation - NeighbourHub";
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
                Text = "📊 Comprehensive Reports & Financial Analytics",
                Font = UIHelper.SubheaderFont,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 16)
            };
            headerPanel.Controls.Add(lblHeader);

            // Filter Bar Panel
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12)
            };

            var lblSelect = new Label { Text = "Select Report:", Location = new Point(16, 12), AutoSize = true, Font = UIHelper.BodyBoldFont };
            cmbReportType = new ComboBox { Location = new Point(16, 36), Size = new Size(330, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbReportType.SelectedIndexChanged += (s, e) => GenerateSelectedReport();

            var lblFrom = new Label { Text = "From Date:", Location = new Point(365, 12), AutoSize = true, Font = UIHelper.BodyBoldFont };
            dtpFrom = new DateTimePicker { Location = new Point(365, 36), Size = new Size(130, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddMonths(-1) };

            var lblTo = new Label { Text = "To Date:", Location = new Point(510, 12), AutoSize = true, Font = UIHelper.BodyBoldFont };
            dtpTo = new DateTimePicker { Location = new Point(510, 36), Size = new Size(130, 28), Format = DateTimePickerFormat.Short, Value = DateTime.Today };

            btnGenerate = new Button { Text = "▶️ Generate", Location = new Point(660, 33), Size = new Size(110, 34) };
            UIHelper.StyleButton(btnGenerate, ThemeColors.Primary, Color.White);
            btnGenerate.Click += (s, e) => GenerateSelectedReport();

            btnExportCsv = new Button { Text = "📥 Export CSV", Location = new Point(780, 33), Size = new Size(120, 34) };
            UIHelper.StyleButton(btnExportCsv, ThemeColors.Success, Color.White);
            btnExportCsv.Click += BtnExportCsv_Click;

            filterPanel.Controls.Add(lblSelect);
            filterPanel.Controls.Add(cmbReportType);
            filterPanel.Controls.Add(lblFrom);
            filterPanel.Controls.Add(dtpFrom);
            filterPanel.Controls.Add(lblTo);
            filterPanel.Controls.Add(dtpTo);
            filterPanel.Controls.Add(btnGenerate);
            filterPanel.Controls.Add(btnExportCsv);

            // Bottom Status / Summary Bar
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 12)
            };

            lblReportStats = new Label
            {
                Text = "Ready to generate report.",
                Font = UIHelper.BodyBoldFont,
                ForeColor = ThemeColors.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 12)
            };
            bottomPanel.Controls.Add(lblReportStats);

            // Center DataGridView
            var centerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Background,
                Padding = new Padding(16)
            };

            dgvReportResults = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvReportResults);

            centerPanel.Controls.Add(dgvReportResults);

            this.Controls.Add(centerPanel);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(filterPanel);
            this.Controls.Add(headerPanel);
        }

        private void PopulateReportTypes()
        {
            string role = SessionManager.CurrentUser?.Role ?? "Admin";

            cmbReportType.Items.Clear();

            if (role == "Admin")
            {
                cmbReportType.Items.Add("User Accounts & Roles Report");
                cmbReportType.Items.Add("Building & Flat Occupancy Report");
                cmbReportType.Items.Add("Payment & Revenue Collection Report");
                cmbReportType.Items.Add("Complaints & Helpdesk Status Report");
                cmbReportType.Items.Add("Maintenance Expenses Report");
            }
            else if (role == "Property Owner")
            {
                cmbReportType.Items.Add("Income & Rent Collection Report");
                cmbReportType.Items.Add("Tenant Lease Directory Report");
                cmbReportType.Items.Add("Vacant Flats Summary Report");
                cmbReportType.Items.Add("Payment History Report");
            }
            else if (role == "Building Manager")
            {
                cmbReportType.Items.Add("Building Residents Directory");
                cmbReportType.Items.Add("Maintenance & Servicing Log");
                cmbReportType.Items.Add("Visitor Entry Log Report");
                cmbReportType.Items.Add("Complaint Status Report");
                cmbReportType.Items.Add("Utility Bills Report");
            }
            else // Resident
            {
                cmbReportType.Items.Add("My Rent & Billing History");
                cmbReportType.Items.Add("My Payment Receipts");
                cmbReportType.Items.Add("My Submitted Complaints");
                cmbReportType.Items.Add("Active Notices Report");
            }

            if (cmbReportType.Items.Count > 0)
                cmbReportType.SelectedIndex = 0;
        }

        private void GenerateSelectedReport()
        {
            string reportName = cmbReportType.SelectedItem?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(reportName)) return;

            try
            {
                string query = string.Empty;
                var parameters = new System.Collections.Generic.List<SqlParameter>();

                switch (reportName)
                {
                    case "User Accounts & Roles Report":
                        query = "SELECT UserId, Username, FullName, Role, Email, Phone, Status, CreatedAt FROM dbo.Users ORDER BY Role, UserId";
                        break;

                    case "Building & Flat Occupancy Report":
                        query = @"
                            SELECT p.PropertyName, b.BuildingName, f.FlatNumber, f.FloorNumber, 
                                   f.MonthlyRent, f.Status, ISNULL(u.FullName, 'None') AS TenantName
                            FROM dbo.Flats f
                            INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                            INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId
                            LEFT JOIN dbo.Tenants t ON f.FlatId = t.FlatId AND t.Status = 'Active'
                            LEFT JOIN dbo.Users u ON t.UserId = u.UserId
                            ORDER BY p.PropertyName, b.BuildingName, f.FlatNumber";
                        break;

                    case "Payment & Revenue Collection Report":
                    case "Payment History Report":
                    case "Income & Rent Collection Report":
                        query = @"
                            SELECT p.PaymentId, u.FullName AS TenantName, f.FlatNumber, b.BuildingName,
                                   p.AmountPaid, p.PaymentDate, p.PaymentMethod, p.TransactionRef, p.PaidBy, p.Status
                            FROM dbo.Payments p
                            INNER JOIN dbo.Tenants t ON p.TenantId = t.TenantId
                            INNER JOIN dbo.Users u ON t.UserId = u.UserId
                            INNER JOIN dbo.Flats f ON p.FlatId = f.FlatId
                            INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                            INNER JOIN dbo.Properties pr ON b.PropertyId = pr.PropertyId
                            WHERE p.PaymentDate BETWEEN @FromDate AND @ToDate ";

                        if (SessionManager.CurrentUser?.Role == "Property Owner")
                        {
                            query += " AND pr.OwnerUserId = @OwnerId ";
                            parameters.Add(new SqlParameter("@OwnerId", SessionManager.CurrentUser.UserId));
                        }

                        parameters.Add(new SqlParameter("@FromDate", dtpFrom.Value.Date));
                        parameters.Add(new SqlParameter("@ToDate", dtpTo.Value.Date.AddDays(1).AddTicks(-1)));
                        query += " ORDER BY p.PaymentDate DESC";
                        break;

                    case "Tenant Lease Directory Report":
                        query = @"
                            SELECT t.TenantId, u.FullName AS TenantName, u.Phone, u.Email,
                                   f.FlatNumber, b.BuildingName, t.AgreedRent, t.SecurityDeposit,
                                   t.LeaseStartDate, t.LeaseEndDate, t.Status
                            FROM dbo.Tenants t
                            INNER JOIN dbo.Users u ON t.UserId = u.UserId
                            INNER JOIN dbo.Flats f ON t.FlatId = f.FlatId
                            INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                            INNER JOIN dbo.Properties pr ON b.PropertyId = pr.PropertyId ";

                        if (SessionManager.CurrentUser?.Role == "Property Owner")
                        {
                            query += " WHERE pr.OwnerUserId = @OwnerId ";
                            parameters.Add(new SqlParameter("@OwnerId", SessionManager.CurrentUser.UserId));
                        }
                        query += " ORDER BY t.TenantId DESC";
                        break;

                    case "Vacant Flats Summary Report":
                        query = @"
                            SELECT f.FlatId, f.FlatNumber, f.FloorNumber, b.BuildingName, pr.PropertyName,
                                   f.Bedrooms, f.Bathrooms, f.AreaSqFt, f.MonthlyRent, f.Status, f.Description
                            FROM dbo.Flats f
                            INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                            INNER JOIN dbo.Properties pr ON b.PropertyId = pr.PropertyId
                            WHERE f.Status = 'Vacant' ";

                        if (SessionManager.CurrentUser?.Role == "Property Owner")
                        {
                            query += " AND pr.OwnerUserId = @OwnerId ";
                            parameters.Add(new SqlParameter("@OwnerId", SessionManager.CurrentUser.UserId));
                        }
                        query += " ORDER BY pr.PropertyName, b.BuildingName, f.FlatNumber";
                        break;

                    case "Complaints & Helpdesk Status Report":
                    case "Complaint Status Report":
                        query = @"
                            SELECT c.ComplaintId, c.Title, c.Category, c.Priority, c.Status,
                                   u.FullName AS SubmittedBy, ISNULL(f.FlatNumber, 'Common') AS Flat,
                                   b.BuildingName, ISNULL(m.FullName, 'Unassigned') AS AssignedTo,
                                   c.SubmittedDate, c.ResolvedDate
                            FROM dbo.Complaints c
                            INNER JOIN dbo.Users u ON c.UserId = u.UserId
                            INNER JOIN dbo.Buildings b ON c.BuildingId = b.BuildingId
                            LEFT JOIN dbo.Flats f ON c.FlatId = f.FlatId
                            LEFT JOIN dbo.Users m ON c.AssignedToManagerId = m.UserId
                            ORDER BY c.ComplaintId DESC";
                        break;

                    case "Maintenance Expenses Report":
                    case "Maintenance & Servicing Log":
                        query = @"
                            SELECT m.MaintenanceId, m.Title, b.BuildingName, m.Cost, m.VendorName,
                                   m.ScheduledDate, m.CompletionDate, m.Status
                            FROM dbo.Maintenance m
                            INNER JOIN dbo.Buildings b ON m.BuildingId = b.BuildingId
                            ORDER BY m.MaintenanceId DESC";
                        break;

                    case "Building Residents Directory":
                        query = @"
                            SELECT r.ResidentId, u.FullName AS ResidentName, u.Phone, u.Email,
                                   f.FlatNumber, b.BuildingName, r.Relationship, r.Profession, r.MoveInDate
                            FROM dbo.Residents r
                            INNER JOIN dbo.Users u ON r.UserId = u.UserId
                            INNER JOIN dbo.Flats f ON r.FlatId = f.FlatId
                            INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                            ORDER BY b.BuildingName, f.FlatNumber";
                        break;

                    case "Visitor Entry Log Report":
                        query = @"
                            SELECT v.VisitorId, v.VisitorName, v.Phone, f.FlatNumber, b.BuildingName,
                                   v.Purpose, v.CheckInTime, v.CheckOutTime, v.Status, u.FullName AS HostResident
                            FROM dbo.Visitors v
                            INNER JOIN dbo.Flats f ON v.FlatId = f.FlatId
                            INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                            INNER JOIN dbo.Users u ON v.ResidentUserId = u.UserId
                            WHERE v.CheckInTime BETWEEN @FromDate AND @ToDate
                            ORDER BY v.CheckInTime DESC";
                        parameters.Add(new SqlParameter("@FromDate", dtpFrom.Value.Date));
                        parameters.Add(new SqlParameter("@ToDate", dtpTo.Value.Date.AddDays(1).AddTicks(-1)));
                        break;

                    case "My Rent & Billing History":
                        query = @"
                            SELECT r.RentId, r.Month, r.Year, r.RentAmount, r.UtilityCharges,
                                   (r.RentAmount + r.UtilityCharges) AS TotalBill, r.DueDate, r.Status
                            FROM dbo.Rent r
                            INNER JOIN dbo.Tenants t ON r.TenantId = t.TenantId
                            WHERE t.UserId = @UserId
                            ORDER BY r.RentId DESC";
                        parameters.Add(new SqlParameter("@UserId", SessionManager.CurrentUser?.UserId ?? 0));
                        break;

                    case "My Payment Receipts":
                        query = @"
                            SELECT p.PaymentId, p.AmountPaid, p.PaymentDate, p.PaymentMethod,
                                   p.TransactionRef, p.PaidBy, p.Status, p.Remarks
                            FROM dbo.Payments p
                            INNER JOIN dbo.Tenants t ON p.TenantId = t.TenantId
                            WHERE t.UserId = @UserId
                            ORDER BY p.PaymentDate DESC";
                        parameters.Add(new SqlParameter("@UserId", SessionManager.CurrentUser?.UserId ?? 0));
                        break;

                    case "My Submitted Complaints":
                        query = @"
                            SELECT c.ComplaintId, c.Title, c.Category, c.Priority, c.Status,
                                   c.SubmittedDate, c.ResolvedDate, c.ResolutionNotes
                            FROM dbo.Complaints c
                            WHERE c.UserId = @UserId
                            ORDER BY c.ComplaintId DESC";
                        parameters.Add(new SqlParameter("@UserId", SessionManager.CurrentUser?.UserId ?? 0));
                        break;

                    case "Active Notices Report":
                        query = @"
                            SELECT n.NoticeId, n.Title, n.Category, n.Priority, b.BuildingName,
                                   u.FullName AS PublishedBy, n.PublishedDate, n.ExpiryDate
                            FROM dbo.Notices n
                            INNER JOIN dbo.Buildings b ON n.BuildingId = b.BuildingId
                            INNER JOIN dbo.Users u ON n.PublishedByUserId = u.UserId
                            WHERE n.IsActive = 1
                            ORDER BY n.PublishedDate DESC";
                        break;

                    default:
                        query = "SELECT 'Report query configured.' AS Information";
                        break;
                }

                currentReportData = DbHelper.ExecuteDataTable(query, parameters.ToArray());
                dgvReportResults.DataSource = currentReportData;

                // Calculate summary stats
                int totalRows = currentReportData.Rows.Count;
                decimal numericSum = 0;
                string sumColumnName = string.Empty;

                if (currentReportData.Columns.Contains("AmountPaid")) sumColumnName = "AmountPaid";
                else if (currentReportData.Columns.Contains("Cost")) sumColumnName = "Cost";
                else if (currentReportData.Columns.Contains("MonthlyRent")) sumColumnName = "MonthlyRent";
                else if (currentReportData.Columns.Contains("AgreedRent")) sumColumnName = "AgreedRent";
                else if (currentReportData.Columns.Contains("TotalBill")) sumColumnName = "TotalBill";

                if (!string.IsNullOrEmpty(sumColumnName))
                {
                    foreach (DataRow row in currentReportData.Rows)
                    {
                        if (row[sumColumnName] != DBNull.Value)
                            numericSum += Convert.ToDecimal(row[sumColumnName]);
                    }
                    lblReportStats.Text = $"📊 Total Records: {totalRows} | Total {sumColumnName}: ৳{numericSum:N0}";
                }
                else
                {
                    lblReportStats.Text = $"📊 Total Records: {totalRows}";
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error generating report: {ex.Message}");
            }
        }

        private void BtnExportCsv_Click(object? sender, EventArgs e)
        {
            if (currentReportData == null || currentReportData.Rows.Count == 0)
            {
                UIHelper.ShowWarning("No report data available to export. Please generate a report first.");
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = $"NeighbourHub_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sb = new StringBuilder();

                    // Column headers
                    for (int i = 0; i < currentReportData.Columns.Count; i++)
                    {
                        sb.Append("\"" + currentReportData.Columns[i].ColumnName + "\"");
                        if (i < currentReportData.Columns.Count - 1) sb.Append(",");
                    }
                    sb.AppendLine();

                    // Rows
                    foreach (DataRow row in currentReportData.Rows)
                    {
                        for (int i = 0; i < currentReportData.Columns.Count; i++)
                        {
                            string val = row[i]?.ToString()?.Replace("\"", "\"\"") ?? string.Empty;
                            sb.Append("\"" + val + "\"");
                            if (i < currentReportData.Columns.Count - 1) sb.Append(",");
                        }
                        sb.AppendLine();
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    UIHelper.ShowSuccess($"Report exported successfully to:\n{sfd.FileName}");
                }
                catch (Exception ex)
                {
                    UIHelper.ShowError($"Export error: {ex.Message}");
                }
            }
        }
    }
}
