using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class ManagerDashboard : Form
    {
        private Panel sidebarPanel = null!;
        private Panel headerPanel = null!;
        private Panel mainContentPanel = null!;
        private Panel dashboardHomeView = null!;
        private Form? currentChildForm;

        // Metric Card Labels
        private Label lblValResidents = null!;
        private Label lblValFlats = null!;
        private Label lblValComplaints = null!;
        private Label lblValMaintenance = null!;
        private Label lblValVisitors = null!;
        private Label lblValNotices = null!;

        private DataGridView dgvActiveComplaints = null!;
        private DataGridView dgvTodayVisitors = null!;

        public ManagerDashboard()
        {
            InitializeComponent();
            LoadManagerMetrics();
        }

        private void InitializeComponent()
        {
            this.Text = "NeighbourHub - Building Manager Operations Portal";
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
                Text = "Building Manager Operations Center",
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
                Text = " BUILDING MANAGER ",
                Font = UIHelper.BodyBoldFont,
                BackColor = ThemeColors.Info,
                ForeColor = Color.White,
                Padding = new Padding(6, 4, 6, 4),
                AutoSize = true,
                Margin = new Padding(10, 4, 0, 0)
            };

            var lblUserName = new Label
            {
                Text = $"👤 {SessionManager.CurrentUser?.FullName ?? "Manager"}",
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
            AddSidebarButton("🏢 Building Details", () => OpenChildForm(new BuildingManagementForm()));
            AddSidebarButton("🚪 Flats & Units", () => OpenChildForm(new FlatManagementForm()));
            AddSidebarButton("👨‍👩‍👧‍👦 Residents Directory", () => OpenChildForm(new ResidentManagementForm()));
            AddSidebarButton("📢 Notices & Alerts", () => OpenChildForm(new NoticeManagementForm()));
            AddSidebarButton("🛠️ Complaints Helpdesk", () => OpenChildForm(new ComplaintManagementForm()));
            AddSidebarButton("🚪 Visitor Gate Log", () => OpenChildForm(new VisitorManagementForm()));
            AddSidebarButton("🔧 Maintenance & Repairs", () => OpenChildForm(new MaintenanceManagementForm()));
            AddSidebarButton("⚡ Utilities Tracking", () => OpenChildForm(new UtilityManagementForm()));
            AddSidebarButton("🚨 Emergency Contacts", () => OpenChildForm(new EmergencyContactForm()));
            AddSidebarButton("📊 Building Reports", () => OpenChildForm(new ReportsForm()));
            AddSidebarButton("👤 Profile", () => new ProfileForm().ShowDialog());

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
                Text = "Building Operations & Daily Management Dashboard",
                Font = UIHelper.HeaderFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblSub = new Label
            {
                Text = "Monitor daily visitor movements, resolve resident complaints, supervise maintenance and publish announcements.",
                Font = UIHelper.BodyFont,
                ForeColor = ThemeColors.TextSecondary,
                Location = new Point(22, 48),
                AutoSize = true
            };

            var btnRefresh = new Button
            {
                Text = "🔄 Refresh Operations",
                Location = new Point(920, 20),
                Size = new Size(160, 34)
            };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => LoadManagerMetrics();

            // Cards Flow Panel
            var cardsFlow = new FlowLayoutPanel
            {
                Location = new Point(20, 80),
                Size = new Size(1080, 120),
                AutoSize = true,
                WrapContents = true
            };

            var pnlRes = UIHelper.CreateMetricCard("Total Residents", "0", ThemeColors.Primary, "Registered occupants");
            lblValResidents = (Label)pnlRes.Controls[1];

            var pnlFlats = UIHelper.CreateMetricCard("Building Flats", "0", Color.FromArgb(14, 165, 233), "Managed units");
            lblValFlats = (Label)pnlFlats.Controls[1];

            var pnlComp = UIHelper.CreateMetricCard("Pending Complaints", "0", ThemeColors.Danger, "Action required");
            lblValComplaints = (Label)pnlComp.Controls[1];

            var pnlMaint = UIHelper.CreateMetricCard("Maintenance Tasks", "0", ThemeColors.Warning, "Active & scheduled");
            lblValMaintenance = (Label)pnlMaint.Controls[1];

            var pnlVis = UIHelper.CreateMetricCard("Visitors Today", "0", ThemeColors.Success, "Gate check-ins");
            lblValVisitors = (Label)pnlVis.Controls[1];

            var pnlNotices = UIHelper.CreateMetricCard("Active Notices", "0", ThemeColors.Purple, "Published bulletins");
            lblValNotices = (Label)pnlNotices.Controls[1];

            cardsFlow.Controls.Add(pnlRes);
            cardsFlow.Controls.Add(pnlFlats);
            cardsFlow.Controls.Add(pnlComp);
            cardsFlow.Controls.Add(pnlMaint);
            cardsFlow.Controls.Add(pnlVis);
            cardsFlow.Controls.Add(pnlNotices);

            // Tables
            int top = 220;

            var lblCompTitle = new Label { Text = "Pending & In-Progress Complaints", Font = UIHelper.SubheaderFont, ForeColor = ThemeColors.TextPrimary, Location = new Point(20, top), AutoSize = true };
            dgvActiveComplaints = new DataGridView { Location = new Point(20, top + 30), Size = new Size(520, 300) };
            UIHelper.StyleDataGridView(dgvActiveComplaints);

            var lblVisTitle = new Label { Text = "Recent Visitor Entry Log", Font = UIHelper.SubheaderFont, ForeColor = ThemeColors.TextPrimary, Location = new Point(560, top), AutoSize = true };
            dgvTodayVisitors = new DataGridView { Location = new Point(560, top + 30), Size = new Size(520, 300) };
            UIHelper.StyleDataGridView(dgvTodayVisitors);

            dashboardHomeView.Controls.Add(lblWelcome);
            dashboardHomeView.Controls.Add(lblSub);
            dashboardHomeView.Controls.Add(btnRefresh);
            dashboardHomeView.Controls.Add(cardsFlow);
            dashboardHomeView.Controls.Add(lblCompTitle);
            dashboardHomeView.Controls.Add(dgvActiveComplaints);
            dashboardHomeView.Controls.Add(lblVisTitle);
            dashboardHomeView.Controls.Add(dgvTodayVisitors);
        }

        private void LoadManagerMetrics()
        {
            try
            {
                lblValResidents.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Residents WHERE Status = 'Active'")?.ToString() ?? "0";
                lblValFlats.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Flats")?.ToString() ?? "0";
                lblValComplaints.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Complaints WHERE Status IN ('Pending', 'In Progress')")?.ToString() ?? "0";
                lblValMaintenance.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Maintenance WHERE Status IN ('Scheduled', 'In Progress')")?.ToString() ?? "0";
                lblValVisitors.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Visitors WHERE CAST(CheckInTime AS DATE) = CAST(GETDATE() AS DATE)")?.ToString() ?? "0";
                lblValNotices.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Notices WHERE IsActive = 1")?.ToString() ?? "0";

                // Active complaints table
                var dtComplaints = DbHelper.ExecuteDataTable(@"
                    SELECT TOP 5 c.Title, c.Category, c.Priority, c.Status, u.FullName AS SubmittedBy, ISNULL(f.FlatNumber, 'Common') AS Flat
                    FROM dbo.Complaints c
                    INNER JOIN dbo.Users u ON c.UserId = u.UserId
                    LEFT JOIN dbo.Flats f ON c.FlatId = f.FlatId
                    WHERE c.Status IN ('Pending', 'In Progress')
                    ORDER BY c.ComplaintId DESC");
                dgvActiveComplaints.DataSource = dtComplaints;

                // Today visitors table
                var dtVisitors = DbHelper.ExecuteDataTable(@"
                    SELECT TOP 5 v.VisitorName, v.Phone, f.FlatNumber, v.Purpose, v.CheckInTime, v.Status
                    FROM dbo.Visitors v
                    INNER JOIN dbo.Flats f ON v.FlatId = f.FlatId
                    ORDER BY v.VisitorId DESC");
                dgvTodayVisitors.DataSource = dtVisitors;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading manager metrics: {ex.Message}");
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
            LoadManagerMetrics();
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
