using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class AdminDashboard : Form
    {
        private Panel sidebarPanel = null!;
        private Panel headerPanel = null!;
        private Panel mainContentPanel = null!;
        private Panel dashboardHomeView = null!;
        private Form? currentChildForm;

        // Metric Card Labels
        private Label lblValUsers = null!;
        private Label lblValBuildings = null!;
        private Label lblValProperties = null!;
        private Label lblValFlats = null!;
        private Label lblValResidents = null!;
        private Label lblValComplaints = null!;
        private Label lblValPayments = null!;

        private DataGridView dgvRecentComplaints = null!;
        private DataGridView dgvRecentPayments = null!;

        public AdminDashboard()
        {
            InitializeComponent();
            LoadDashboardMetrics();
        }

        private void InitializeComponent()
        {
            this.Text = "NeighbourHub - System Administrator Dashboard";
            this.Size = new Size(1366, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeColors.Background;
            this.Font = UIHelper.BodyFont;
            this.MinimumSize = new Size(1100, 650);

            // 1. Top Header Panel
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
                Text = "Smart Apartment Management System",
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
                Text = " ADMIN ",
                Font = UIHelper.BodyBoldFont,
                BackColor = ThemeColors.Primary,
                ForeColor = Color.White,
                Padding = new Padding(6, 4, 6, 4),
                AutoSize = true,
                Margin = new Padding(10, 4, 0, 0)
            };

            var lblUserName = new Label
            {
                Text = $"👤 {SessionManager.CurrentUser?.FullName ?? "Administrator"}",
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

            // 2. Left Sidebar Navigation Panel
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = ThemeColors.SidebarDark,
                Padding = new Padding(0, 10, 0, 10),
                AutoScroll = true
            };

            AddSidebarButton("📊 Dashboard", () => ShowDashboardHome());
            AddSidebarButton("👥 Users", () => OpenChildForm(new UserManagementForm()));
            AddSidebarButton("🏢 Buildings", () => OpenChildForm(new BuildingManagementForm()));
            AddSidebarButton("🏙️ Properties", () => OpenChildForm(new PropertyManagementForm()));
            AddSidebarButton("🚪 Flats & Units", () => OpenChildForm(new FlatManagementForm()));
            AddSidebarButton("📝 Tenants & Leases", () => OpenChildForm(new TenantManagementForm()));
            AddSidebarButton("👨‍👩‍👧‍👦 Residents", () => OpenChildForm(new ResidentManagementForm()));
            AddSidebarButton("💰 Rent Billing", () => OpenChildForm(new RentManagementForm()));
            AddSidebarButton("💳 Payments", () => OpenChildForm(new PaymentManagementForm()));
            AddSidebarButton("🛠️ Complaints", () => OpenChildForm(new ComplaintManagementForm()));
            AddSidebarButton("📢 Notices", () => OpenChildForm(new NoticeManagementForm()));
            AddSidebarButton("🔧 Maintenance", () => OpenChildForm(new MaintenanceManagementForm()));
            AddSidebarButton("📊 Reports", () => OpenChildForm(new ReportsForm()));
            AddSidebarButton("👤 My Profile", () => new ProfileForm().ShowDialog());

            // 3. Main Content Container Panel
            mainContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Background
            };

            // Build Default Dashboard Home View
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
                Text = "System Administrator Dashboard",
                Font = UIHelper.HeaderFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblSub = new Label
            {
                Text = "Live overview of system users, apartment properties, units, complaints, and financial collections.",
                Font = UIHelper.BodyFont,
                ForeColor = ThemeColors.TextSecondary,
                Location = new Point(22, 48),
                AutoSize = true
            };

            var btnRefresh = new Button
            {
                Text = "🔄 Refresh Metrics",
                Location = new Point(940, 20),
                Size = new Size(140, 34)
            };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => LoadDashboardMetrics();

            // Cards Flow Panel
            var cardsFlow = new FlowLayoutPanel
            {
                Location = new Point(20, 80),
                Size = new Size(1080, 120),
                AutoSize = true,
                WrapContents = true
            };

            var pnlUsers = UIHelper.CreateMetricCard("Total Users", "0", ThemeColors.Primary, "Active accounts");
            lblValUsers = (Label)pnlUsers.Controls[1];

            var pnlProperties = UIHelper.CreateMetricCard("Properties", "0", ThemeColors.Purple, "Apartment complexes");
            lblValProperties = (Label)pnlProperties.Controls[1];

            var pnlBuildings = UIHelper.CreateMetricCard("Buildings", "0", ThemeColors.Info, "Total blocks");
            lblValBuildings = (Label)pnlBuildings.Controls[1];

            var pnlFlats = UIHelper.CreateMetricCard("Total Flats", "0", Color.FromArgb(14, 165, 233), "Apartment units");
            lblValFlats = (Label)pnlFlats.Controls[1];

            var pnlResidents = UIHelper.CreateMetricCard("Residents", "0", ThemeColors.Success, "Active occupants");
            lblValResidents = (Label)pnlResidents.Controls[1];

            var pnlComplaints = UIHelper.CreateMetricCard("Pending Complaints", "0", ThemeColors.Danger, "Needs attention");
            lblValComplaints = (Label)pnlComplaints.Controls[1];

            var pnlPayments = UIHelper.CreateMetricCard("Total Collections", "৳0", ThemeColors.Warning, "Payments received");
            lblValPayments = (Label)pnlPayments.Controls[1];

            cardsFlow.Controls.Add(pnlUsers);
            cardsFlow.Controls.Add(pnlProperties);
            cardsFlow.Controls.Add(pnlBuildings);
            cardsFlow.Controls.Add(pnlFlats);
            cardsFlow.Controls.Add(pnlResidents);
            cardsFlow.Controls.Add(pnlComplaints);
            cardsFlow.Controls.Add(pnlPayments);

            // Recent Tables Grid
            int top = 220;

            var lblCompTitle = new Label { Text = "Recent Resident Complaints", Font = UIHelper.SubheaderFont, ForeColor = ThemeColors.TextPrimary, Location = new Point(20, top), AutoSize = true };
            dgvRecentComplaints = new DataGridView { Location = new Point(20, top + 30), Size = new Size(520, 300) };
            UIHelper.StyleDataGridView(dgvRecentComplaints);

            var lblPayTitle = new Label { Text = "Recent Rent Payments Collected", Font = UIHelper.SubheaderFont, ForeColor = ThemeColors.TextPrimary, Location = new Point(560, top), AutoSize = true };
            dgvRecentPayments = new DataGridView { Location = new Point(560, top + 30), Size = new Size(520, 300) };
            UIHelper.StyleDataGridView(dgvRecentPayments);

            dashboardHomeView.Controls.Add(lblWelcome);
            dashboardHomeView.Controls.Add(lblSub);
            dashboardHomeView.Controls.Add(btnRefresh);
            dashboardHomeView.Controls.Add(cardsFlow);
            dashboardHomeView.Controls.Add(lblCompTitle);
            dashboardHomeView.Controls.Add(dgvRecentComplaints);
            dashboardHomeView.Controls.Add(lblPayTitle);
            dashboardHomeView.Controls.Add(dgvRecentPayments);
        }

        private void LoadDashboardMetrics()
        {
            try
            {
                lblValUsers.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Users")?.ToString() ?? "0";
                lblValProperties.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Properties")?.ToString() ?? "0";
                lblValBuildings.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Buildings")?.ToString() ?? "0";
                lblValFlats.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Flats")?.ToString() ?? "0";
                lblValResidents.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Residents WHERE Status = 'Active'")?.ToString() ?? "0";
                lblValComplaints.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Complaints WHERE Status IN ('Pending', 'In Progress')")?.ToString() ?? "0";

                object? sumPay = DbHelper.ExecuteScalar("SELECT ISNULL(SUM(AmountPaid), 0) FROM dbo.Payments");
                decimal tot = Convert.ToDecimal(sumPay ?? 0);
                lblValPayments.Text = $"৳{tot:N0}";

                // Recent Complaints
                var dtComplaints = DbHelper.ExecuteDataTable(@"
                    SELECT TOP 5 c.Title, c.Category, c.Priority, c.Status, u.FullName AS Resident
                    FROM dbo.Complaints c
                    INNER JOIN dbo.Users u ON c.UserId = u.UserId
                    ORDER BY c.ComplaintId DESC");
                dgvRecentComplaints.DataSource = dtComplaints;

                // Recent Payments
                var dtPayments = DbHelper.ExecuteDataTable(@"
                    SELECT TOP 5 u.FullName AS Tenant, f.FlatNumber, p.AmountPaid AS Amount, p.PaymentMethod, p.PaymentDate
                    FROM dbo.Payments p
                    INNER JOIN dbo.Tenants t ON p.TenantId = t.TenantId
                    INNER JOIN dbo.Users u ON t.UserId = u.UserId
                    INNER JOIN dbo.Flats f ON p.FlatId = f.FlatId
                    ORDER BY p.PaymentId DESC");
                dgvRecentPayments.DataSource = dtPayments;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading admin metrics: {ex.Message}");
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
            LoadDashboardMetrics();
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
