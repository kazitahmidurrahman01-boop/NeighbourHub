using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class ResidentDashboard : Form
    {
        private Panel sidebarPanel = null!;
        private Panel headerPanel = null!;
        private Panel mainContentPanel = null!;
        private Panel dashboardHomeView = null!;
        private Form? currentChildForm;

        // Metric Card Labels
        private Label lblValFlat = null!;
        private Label lblValRent = null!;
        private Label lblValPayStatus = null!;
        private Label lblValComplaints = null!;
        private Label lblValNotices = null!;
        private Label lblValMaint = null!;

        private DataGridView dgvMyComplaints = null!;
        private DataGridView dgvActiveNotices = null!;

        public ResidentDashboard()
        {
            InitializeComponent();
            LoadResidentMetrics();
        }

        private void InitializeComponent()
        {
            this.Text = "NeighbourHub - Resident & Tenant Portal";
            this.Size = new Size(1366, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeColors.Background;
            this.Font = UIHelper.BodyFont;
            this.MinimumSize = new Size(1100, 650);

            // 1. Header
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = ThemeColors.HeaderDark,
                Padding = new Padding(20, 12, 20, 12)
            };

            var lblAppTitle = new Label
            {
                Text = "🏢 NeighbourHub",
                Font = UIHelper.HeaderFont,
                ForeColor = Color.White,
                Location = new Point(20, 14),
                AutoSize = true
            };

            var lblTagline = new Label
            {
                Text = "Resident & Tenant Community Portal",
                Font = UIHelper.SmallFont,
                ForeColor = ThemeColors.PrimaryLight,
                Location = new Point(220, 24),
                AutoSize = true
            };

            var pnlUserInfo = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 4, 0, 0)
            };

            var btnLogout = new Button
            {
                Text = "🚪 Logout",
                Size = new Size(95, 34),
                Margin = new Padding(12, 0, 0, 0)
            };
            UIHelper.StyleButton(btnLogout, ThemeColors.Danger, Color.White);
            btnLogout.Click += (s, e) => this.Close();

            var lblRoleBadge = new Label
            {
                Text = " RESIDENT ",
                Font = UIHelper.BodyBoldFont,
                BackColor = ThemeColors.Success,
                ForeColor = Color.White,
                Padding = new Padding(6, 4, 6, 4),
                AutoSize = true,
                Margin = new Padding(10, 4, 0, 0)
            };

            var lblUserName = new Label
            {
                Text = $"👤 {SessionManager.CurrentUser?.FullName ?? "Resident"}",
                Font = UIHelper.BodyBoldFont,
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 0)
            };

            pnlUserInfo.Controls.Add(btnLogout);
            pnlUserInfo.Controls.Add(lblRoleBadge);
            pnlUserInfo.Controls.Add(lblUserName);

            headerPanel.Controls.Add(lblAppTitle);
            headerPanel.Controls.Add(lblTagline);
            headerPanel.Controls.Add(pnlUserInfo);

            // 2. Sidebar
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = ThemeColors.SidebarDark,
                Padding = new Padding(0, 10, 0, 10),
                AutoScroll = true
            };

            AddSidebarButton("📊 Dashboard", () => ShowDashboardHome());
            AddSidebarButton("🚪 My Flat Details", () => ShowMyFlatModal());
            AddSidebarButton("👨‍👩‍👧‍👦 Neighbors Directory", () => OpenChildForm(new ResidentManagementForm()));
            AddSidebarButton("📢 Notice Board", () => OpenChildForm(new NoticeManagementForm()));
            AddSidebarButton("🛠️ Submit Complaint", () => OpenChildForm(new ComplaintManagementForm()));
            AddSidebarButton("🚪 Visitor Passes", () => OpenChildForm(new VisitorManagementForm()));
            AddSidebarButton("💰 My Rent Bills", () => OpenChildForm(new RentManagementForm()));
            AddSidebarButton("💳 Payment History", () => OpenChildForm(new PaymentManagementForm()));
            AddSidebarButton("🤝 Community Sharing", () => OpenChildForm(new SharedItemManagementForm()));
            AddSidebarButton("🚨 Emergency Contacts", () => OpenChildForm(new EmergencyContactForm()));
            AddSidebarButton("👤 My Profile", () => new ProfileForm().ShowDialog());

            // 3. Main Content Container
            mainContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Background
            };

            BuildDashboardHomeView();
            mainContentPanel.Controls.Add(dashboardHomeView);

            this.Controls.Add(mainContentPanel);
            this.Controls.Add(sidebarPanel);
            this.Controls.Add(headerPanel);
        }

        private void AddSidebarButton(string text, Action onClick)
        {
            var btn = new Button
            {
                Text = "  " + text,
                Dock = DockStyle.Top,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = ThemeColors.SidebarDark,
                ForeColor = Color.FromArgb(226, 232, 240),
                Font = UIHelper.BodyFont,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            btn.MouseEnter += (s, e) => { if (btn.BackColor != ThemeColors.SidebarActive) btn.BackColor = ThemeColors.SidebarHover; };
            btn.MouseLeave += (s, e) => { if (btn.BackColor != ThemeColors.SidebarActive) btn.BackColor = ThemeColors.SidebarDark; };
            btn.Click += (s, e) =>
            {
                ResetSidebarSelection();
                btn.BackColor = ThemeColors.SidebarActive;
                btn.ForeColor = Color.White;
                onClick();
            };

            sidebarPanel.Controls.Add(btn);
            sidebarPanel.Controls.SetChildIndex(btn, 0);
        }

        private void ResetSidebarSelection()
        {
            foreach (Control c in sidebarPanel.Controls)
            {
                if (c is Button b)
                {
                    b.BackColor = ThemeColors.SidebarDark;
                    b.ForeColor = Color.FromArgb(226, 232, 240);
                }
            }
        }

        private void BuildDashboardHomeView()
        {
            dashboardHomeView = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var lblWelcome = new Label
            {
                Text = $"Welcome, {SessionManager.CurrentUser?.FullName ?? "Resident"}!",
                Font = UIHelper.HeaderFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblSub = new Label
            {
                Text = "Here is your apartment overview, rent status, active notices, and pending service tickets.",
                Font = UIHelper.BodyFont,
                ForeColor = ThemeColors.TextSecondary,
                Location = new Point(22, 48),
                AutoSize = true
            };

            var btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(960, 20),
                Size = new Size(110, 34)
            };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => LoadResidentMetrics();

            // Cards Flow Panel
            var cardsFlow = new FlowLayoutPanel
            {
                Location = new Point(20, 80),
                Size = new Size(1080, 120),
                AutoSize = true,
                WrapContents = true
            };

            var pnlFlat = UIHelper.CreateMetricCard("My Flat", "---", ThemeColors.Primary, "Assigned unit");
            lblValFlat = (Label)pnlFlat.Controls[1];

            var pnlRent = UIHelper.CreateMetricCard("Monthly Rent", "৳0", ThemeColors.Purple, "Agreed rate");
            lblValRent = (Label)pnlRent.Controls[1];

            var pnlStatus = UIHelper.CreateMetricCard("Payment Status", "---", ThemeColors.Success, "Current month");
            lblValPayStatus = (Label)pnlStatus.Controls[1];

            var pnlComp = UIHelper.CreateMetricCard("My Complaints", "0", ThemeColors.Danger, "In progress");
            lblValComplaints = (Label)pnlComp.Controls[1];

            var pnlNotices = UIHelper.CreateMetricCard("New Notices", "0", ThemeColors.Warning, "Active bulletins");
            lblValNotices = (Label)pnlNotices.Controls[1];

            var pnlMaint = UIHelper.CreateMetricCard("Maintenance", "0", ThemeColors.Info, "Upcoming scheduled");
            lblValMaint = (Label)pnlMaint.Controls[1];

            cardsFlow.Controls.Add(pnlFlat);
            cardsFlow.Controls.Add(pnlRent);
            cardsFlow.Controls.Add(pnlStatus);
            cardsFlow.Controls.Add(pnlComp);
            cardsFlow.Controls.Add(pnlNotices);
            cardsFlow.Controls.Add(pnlMaint);

            // Tables
            int top = 220;

            var lblCompTitle = new Label { Text = "My Submitted Complaints Status", Font = UIHelper.SubheaderFont, ForeColor = ThemeColors.TextPrimary, Location = new Point(20, top), AutoSize = true };
            dgvMyComplaints = new DataGridView { Location = new Point(20, top + 30), Size = new Size(520, 300) };
            UIHelper.StyleDataGridView(dgvMyComplaints);

            var lblNoticeTitle = new Label { Text = "Active Society Notices & Alerts", Font = UIHelper.SubheaderFont, ForeColor = ThemeColors.TextPrimary, Location = new Point(560, top), AutoSize = true };
            dgvActiveNotices = new DataGridView { Location = new Point(560, top + 30), Size = new Size(520, 300) };
            UIHelper.StyleDataGridView(dgvActiveNotices);

            dashboardHomeView.Controls.Add(lblWelcome);
            dashboardHomeView.Controls.Add(lblSub);
            dashboardHomeView.Controls.Add(btnRefresh);
            dashboardHomeView.Controls.Add(cardsFlow);
            dashboardHomeView.Controls.Add(lblCompTitle);
            dashboardHomeView.Controls.Add(dgvMyComplaints);
            dashboardHomeView.Controls.Add(lblNoticeTitle);
            dashboardHomeView.Controls.Add(dgvActiveNotices);
        }

        private void LoadResidentMetrics()
        {
            int userId = SessionManager.CurrentUser?.UserId ?? 6;

            try
            {
                // Flat & Rent info
                string tenantInfoQuery = @"
                    SELECT TOP 1 f.FlatNumber, b.BuildingName, t.AgreedRent
                    FROM dbo.Tenants t
                    INNER JOIN dbo.Flats f ON t.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    WHERE t.UserId = @UserId AND t.Status = 'Active'";

                var dtTenant = DbHelper.ExecuteDataTable(tenantInfoQuery, new[] { new SqlParameter("@UserId", userId) });
                if (dtTenant.Rows.Count > 0)
                {
                    lblValFlat.Text = $"Flat {dtTenant.Rows[0]["FlatNumber"]}";
                    decimal rent = Convert.ToDecimal(dtTenant.Rows[0]["AgreedRent"]);
                    lblValRent.Text = $"৳{rent:N0}";
                }
                else
                {
                    lblValFlat.Text = "Not Assigned";
                    lblValRent.Text = "৳0";
                }

                // Latest Rent status
                string latestRentQuery = @"
                    SELECT TOP 1 Status 
                    FROM dbo.Rent r
                    INNER JOIN dbo.Tenants t ON r.TenantId = t.TenantId
                    WHERE t.UserId = @UserId
                    ORDER BY r.RentId DESC";
                object? st = DbHelper.ExecuteScalar(latestRentQuery, new[] { new SqlParameter("@UserId", userId) });
                lblValPayStatus.Text = st?.ToString() ?? "Paid";

                // Complaints count
                lblValComplaints.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Complaints WHERE UserId = @UserId AND Status IN ('Pending', 'In Progress')",
                    new[] { new SqlParameter("@UserId", userId) })?.ToString() ?? "0";

                // Active notices
                lblValNotices.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Notices WHERE IsActive = 1")?.ToString() ?? "0";

                // Maintenance
                lblValMaint.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Maintenance WHERE Status = 'Scheduled'")?.ToString() ?? "0";

                // Complaints table
                var dtMyComplaints = DbHelper.ExecuteDataTable(@"
                    SELECT c.Title, c.Category, c.Priority, c.Status, c.SubmittedDate, ISNULL(c.ResolutionNotes, 'Pending action') AS [Resolution Notes]
                    FROM dbo.Complaints c
                    WHERE c.UserId = @UserId
                    ORDER BY c.ComplaintId DESC", new[] { new SqlParameter("@UserId", userId) });
                dgvMyComplaints.DataSource = dtMyComplaints;

                // Active notices table
                var dtNotices = DbHelper.ExecuteDataTable(@"
                    SELECT n.Title, n.Category, n.Priority, b.BuildingName, n.PublishedDate, n.Content
                    FROM dbo.Notices n
                    INNER JOIN dbo.Buildings b ON n.BuildingId = b.BuildingId
                    WHERE n.IsActive = 1
                    ORDER BY n.PublishedDate DESC");
                dgvActiveNotices.DataSource = dtNotices;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading resident metrics: {ex.Message}");
            }
        }

        private void ShowMyFlatModal()
        {
            int userId = SessionManager.CurrentUser?.UserId ?? 6;
            try
            {
                string query = @"
                    SELECT f.FlatNumber, f.FloorNumber, f.Bedrooms, f.Bathrooms, f.AreaSqFt,
                           b.BuildingName, b.Address, b.SecurityContact, p.PropertyName,
                           t.AgreedRent, t.SecurityDeposit, t.LeaseStartDate, t.LeaseEndDate,
                           ISNULL(m.FullName, 'Office Staff') AS ManagerName,
                           ISNULL(m.Phone, 'N/A') AS ManagerPhone
                    FROM dbo.Tenants t
                    INNER JOIN dbo.Flats f ON t.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId
                    LEFT JOIN dbo.Users m ON b.ManagerUserId = m.UserId
                    WHERE t.UserId = @UserId AND t.Status = 'Active'";

                var dt = DbHelper.ExecuteDataTable(query, new[] { new SqlParameter("@UserId", userId) });
                if (dt.Rows.Count == 0)
                {
                    UIHelper.ShowWarning("No active flat allocation found for your resident account.");
                    return;
                }

                var r = dt.Rows[0];
                string info = $@"🏢 APARTMENT UNIT DETAILS
--------------------------------------------------
Property: {r["PropertyName"]}
Building: {r["BuildingName"]}
Flat Number: Flat {r["FlatNumber"]} (Floor {r["FloorNumber"]})
Layout: {r["Bedrooms"]} Bedrooms, {r["Bathrooms"]} Bathrooms ({r["AreaSqFt"]} Sq Ft)
Address: {r["Address"]}

📋 LEASE & RENT DETAILS
--------------------------------------------------
Monthly Agreed Rent: ৳{Convert.ToDecimal(r["AgreedRent"]):N0}
Security Deposit: ৳{Convert.ToDecimal(r["SecurityDeposit"]):N0}
Lease Start: {Convert.ToDateTime(r["LeaseStartDate"]):yyyy-MM-dd}
Lease End: {Convert.ToDateTime(r["LeaseEndDate"]):yyyy-MM-dd}

📞 BUILDING CONTACTS
--------------------------------------------------
Building Manager: {r["ManagerName"]} ({r["ManagerPhone"]})
Security Gate: {r["SecurityContact"]}";

                MessageBox.Show(info, "My Flat Information - NeighbourHub", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error retrieving flat details: {ex.Message}");
            }
        }

        private void ShowDashboardHome()
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close();
                currentChildForm = null;
            }
            dashboardHomeView.Visible = true;
            dashboardHomeView.BringToFront();
            LoadResidentMetrics();
        }

        private void OpenChildForm(Form childForm)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close();
            }

            dashboardHomeView.Visible = false;
            currentChildForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            mainContentPanel.Controls.Add(childForm);
            childForm.BringToFront();
            childForm.Show();
        }
    }
}
