using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using NeighbourHub.Data;
using NeighbourHub.UI;

namespace NeighbourHub.Forms
{
    public class PropertyOwnerDashboard : Form
    {
        private Panel sidebarPanel = null!;
        private Panel headerPanel = null!;
        private Panel mainContentPanel = null!;
        private Panel dashboardHomeView = null!;
        private Form? currentChildForm;

        // Metric Card Labels
        private Label lblValProperties = null!;
        private Label lblValTotalFlats = null!;
        private Label lblValOccupied = null!;
        private Label lblValVacant = null!;
        private Label lblValExpectedRent = null!;
        private Label lblValCollectedRent = null!;
        private Label lblValDueRent = null!;

        private DataGridView dgvFlatBreakdown = null!;

        public PropertyOwnerDashboard()
        {
            InitializeComponent();
            LoadOwnerMetrics();
        }

        private void InitializeComponent()
        {
            this.Text = "NeighbourHub - Property Owner Financial Dashboard";
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
                Text = "Property Owner Financial Portal",
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
                Text = " PROPERTY OWNER ",
                Font = UIHelper.BodyBoldFont,
                BackColor = ThemeColors.Purple,
                ForeColor = Color.White,
                Padding = new Padding(6, 4, 6, 4),
                AutoSize = true,
                Margin = new Padding(10, 4, 0, 0)
            };

            var lblUserName = new Label
            {
                Text = $"👤 {SessionManager.CurrentUser?.FullName ?? "Property Owner"}",
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

            // 2. Left Sidebar
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = ThemeColors.SidebarDark,
                Padding = new Padding(0, 10, 0, 10),
                AutoScroll = true
            };

            AddSidebarButton("📊 Dashboard", () => ShowDashboardHome());
            AddSidebarButton("🏙️ My Properties", () => OpenChildForm(new PropertyManagementForm()));
            AddSidebarButton("🚪 Flats & Units", () => OpenChildForm(new FlatManagementForm()));
            AddSidebarButton("📝 Tenants & Leases", () => OpenChildForm(new TenantManagementForm()));
            AddSidebarButton("💰 Rent Billing", () => OpenChildForm(new RentManagementForm()));
            AddSidebarButton("💳 Payment History", () => OpenChildForm(new PaymentManagementForm()));
            AddSidebarButton("🚪 Vacant Flats", () => OpenChildForm(new FlatManagementForm()));
            AddSidebarButton("📊 Income Reports", () => OpenChildForm(new ReportsForm()));
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
                Text = "Property Owner Financial & Tenant Overview",
                Font = UIHelper.HeaderFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblSub = new Label
            {
                Text = "Track property occupancy, monthly expected rent, collections, and dues across your buildings.",
                Font = UIHelper.BodyFont,
                ForeColor = ThemeColors.TextSecondary,
                Location = new Point(22, 48),
                AutoSize = true
            };

            var btnRefresh = new Button
            {
                Text = "🔄 Refresh Stats",
                Location = new Point(940, 20),
                Size = new Size(130, 34)
            };
            UIHelper.StyleButton(btnRefresh, ThemeColors.PrimaryLight, ThemeColors.PrimaryDark);
            btnRefresh.Click += (s, e) => LoadOwnerMetrics();

            // Cards Flow Panel
            var cardsFlow = new FlowLayoutPanel
            {
                Location = new Point(20, 80),
                Size = new Size(1080, 120),
                AutoSize = true,
                WrapContents = true
            };

            var pnlProps = UIHelper.CreateMetricCard("My Properties", "0", ThemeColors.Primary, "Owned complexes");
            lblValProperties = (Label)pnlProps.Controls[1];

            var pnlFlats = UIHelper.CreateMetricCard("Total Flats", "0", Color.FromArgb(14, 165, 233), "Across properties");
            lblValTotalFlats = (Label)pnlFlats.Controls[1];

            var pnlOcc = UIHelper.CreateMetricCard("Occupied Flats", "0", ThemeColors.Success, "Active tenants");
            lblValOccupied = (Label)pnlOcc.Controls[1];

            var pnlVac = UIHelper.CreateMetricCard("Vacant Flats", "0", ThemeColors.Danger, "Available for rent");
            lblValVacant = (Label)pnlVac.Controls[1];

            var pnlExp = UIHelper.CreateMetricCard("Expected Monthly Rent", "৳0", ThemeColors.Purple, "Calculated potential");
            lblValExpectedRent = (Label)pnlExp.Controls[1];

            var pnlCol = UIHelper.CreateMetricCard("Collected Rent", "৳0", ThemeColors.Success, "Paid this month");
            lblValCollectedRent = (Label)pnlCol.Controls[1];

            var pnlDue = UIHelper.CreateMetricCard("Due Rent", "৳0", ThemeColors.Warning, "Pending collection");
            lblValDueRent = (Label)pnlDue.Controls[1];

            cardsFlow.Controls.Add(pnlProps);
            cardsFlow.Controls.Add(pnlFlats);
            cardsFlow.Controls.Add(pnlOcc);
            cardsFlow.Controls.Add(pnlVac);
            cardsFlow.Controls.Add(pnlExp);
            cardsFlow.Controls.Add(pnlCol);
            cardsFlow.Controls.Add(pnlDue);

            // Live Flat Rent Breakdown Table
            int top = 220;
            var lblTableTitle = new Label
            {
                Text = "🏢 Flat-by-Flat Tenant & Rent Status Breakdown",
                Font = UIHelper.SubheaderFont,
                ForeColor = ThemeColors.TextPrimary,
                Location = new Point(20, top),
                AutoSize = true
            };

            dgvFlatBreakdown = new DataGridView
            {
                Location = new Point(20, top + 35),
                Size = new Size(1060, 380),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            UIHelper.StyleDataGridView(dgvFlatBreakdown);

            dashboardHomeView.Controls.Add(lblWelcome);
            dashboardHomeView.Controls.Add(lblSub);
            dashboardHomeView.Controls.Add(btnRefresh);
            dashboardHomeView.Controls.Add(cardsFlow);
            dashboardHomeView.Controls.Add(lblTableTitle);
            dashboardHomeView.Controls.Add(dgvFlatBreakdown);
        }

        private void LoadOwnerMetrics()
        {
            int ownerId = SessionManager.CurrentUser?.UserId ?? 2;

            try
            {
                // Properties count
                lblValProperties.Text = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM dbo.Properties WHERE OwnerUserId = @OwnerId",
                    new[] { new SqlParameter("@OwnerId", ownerId) })?.ToString() ?? "0";

                // Flats counts
                string flatCountQuery = @"
                    SELECT 
                        COUNT(*) AS TotalFlats,
                        SUM(CASE WHEN f.Status = 'Occupied' THEN 1 ELSE 0 END) AS OccupiedFlats,
                        SUM(CASE WHEN f.Status = 'Vacant' THEN 1 ELSE 0 END) AS VacantFlats
                    FROM dbo.Flats f
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId
                    WHERE p.OwnerUserId = @OwnerId";

                var dtFlats = DbHelper.ExecuteDataTable(flatCountQuery, new[] { new SqlParameter("@OwnerId", ownerId) });
                if (dtFlats.Rows.Count > 0)
                {
                    lblValTotalFlats.Text = dtFlats.Rows[0]["TotalFlats"].ToString();
                    lblValOccupied.Text = dtFlats.Rows[0]["OccupiedFlats"].ToString();
                    lblValVacant.Text = dtFlats.Rows[0]["VacantFlats"].ToString();
                }

                // Financial metrics
                string finQuery = @"
                    SELECT 
                        ISNULL(SUM(r.RentAmount + r.UtilityCharges), 0) AS ExpectedTotal,
                        ISNULL(SUM(CASE WHEN r.Status = 'Paid' THEN (r.RentAmount + r.UtilityCharges) ELSE 0 END), 0) AS CollectedTotal,
                        ISNULL(SUM(CASE WHEN r.Status <> 'Paid' THEN (r.RentAmount + r.UtilityCharges) ELSE 0 END), 0) AS DueTotal
                    FROM dbo.Rent r
                    INNER JOIN dbo.Flats f ON r.FlatId = f.FlatId
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId
                    WHERE p.OwnerUserId = @OwnerId";

                var dtFin = DbHelper.ExecuteDataTable(finQuery, new[] { new SqlParameter("@OwnerId", ownerId) });
                if (dtFin.Rows.Count > 0)
                {
                    decimal exp = Convert.ToDecimal(dtFin.Rows[0]["ExpectedTotal"]);
                    decimal col = Convert.ToDecimal(dtFin.Rows[0]["CollectedTotal"]);
                    decimal due = Convert.ToDecimal(dtFin.Rows[0]["DueTotal"]);

                    lblValExpectedRent.Text = $"৳{exp:N0}";
                    lblValCollectedRent.Text = $"৳{col:N0}";
                    lblValDueRent.Text = $"৳{due:N0}";
                }

                // Breakdown Table: Flat -> Tenant -> Monthly Rent -> Status (Paid/Due/Vacant)
                string breakdownQuery = @"
                    SELECT 
                        p.PropertyName AS [Property],
                        b.BuildingName AS [Building],
                        ('Flat ' + f.FlatNumber) AS [Flat],
                        f.FloorNumber AS [Floor],
                        ISNULL(tu.FullName, '---') AS [Current Tenant],
                        ISNULL(tu.Phone, '---') AS [Tenant Phone],
                        f.MonthlyRent AS [Monthly Rent],
                        CASE 
                            WHEN f.Status = 'Vacant' THEN 'Vacant'
                            WHEN r.Status = 'Paid' THEN 'Paid'
                            WHEN r.Status = 'Due' THEN 'Due'
                            ELSE f.Status
                        END AS [Rent / Occupancy Status]
                    FROM dbo.Flats f
                    INNER JOIN dbo.Buildings b ON f.BuildingId = b.BuildingId
                    INNER JOIN dbo.Properties p ON b.PropertyId = p.PropertyId
                    LEFT JOIN dbo.Tenants t ON f.FlatId = t.FlatId AND t.Status = 'Active'
                    LEFT JOIN dbo.Users tu ON t.UserId = tu.UserId
                    LEFT JOIN dbo.Rent r ON f.FlatId = r.FlatId AND r.Month = 'August' AND r.Year = 2026
                    WHERE p.OwnerUserId = @OwnerId
                    ORDER BY p.PropertyName, b.BuildingName, f.FloorNumber, f.FlatNumber";

                var dtBreakdown = DbHelper.ExecuteDataTable(breakdownQuery, new[] { new SqlParameter("@OwnerId", ownerId) });
                dgvFlatBreakdown.DataSource = dtBreakdown;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError($"Error loading property owner stats: {ex.Message}");
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
            LoadOwnerMetrics();
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
