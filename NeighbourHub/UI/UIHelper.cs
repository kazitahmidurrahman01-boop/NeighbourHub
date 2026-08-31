using System;
using System.Drawing;
using System.Windows.Forms;

namespace NeighbourHub.UI
{
    /// <summary>
    /// UI Helper with reusable formatting and styling methods for WinForms.
    /// </summary>
    public static class UIHelper
    {
        public static readonly Font HeaderFont = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static readonly Font SubheaderFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        public static readonly Font BodyBoldFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        public static readonly Font SmallFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        public static readonly Font StatNumberFont = new Font("Segoe UI", 18F, FontStyle.Bold);

        /// <summary>
        /// Styles DataGridView for a modern, clean SaaS/enterprise look.
        /// </summary>
        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = ThemeColors.BorderColor;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowTemplate.Height = 36;
            dgv.Font = BodyFont;
            dgv.EnableHeadersVisualStyles = false;

            // Header styling
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.GridHeaderBg;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            dgv.ColumnHeadersDefaultCellStyle.Font = BodyBoldFont;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 8, 6, 8);
            dgv.ColumnHeadersHeight = 40;

            // Row styling
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = ThemeColors.PrimaryLight;
            dgv.DefaultCellStyle.SelectionForeColor = ThemeColors.PrimaryDark;
            dgv.DefaultCellStyle.Padding = new Padding(6, 2, 6, 2);

            // Alternating rows
            dgv.AlternatingRowsDefaultCellStyle.BackColor = ThemeColors.GridRowAlternate;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = ThemeColors.TextPrimary;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = ThemeColors.PrimaryLight;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = ThemeColors.PrimaryDark;
        }

        /// <summary>
        /// Styles standard buttons with flat appearance and hover effects.
        /// </summary>
        public static void StyleButton(Button btn, Color backColor, Color foreColor)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = backColor;
            btn.ForeColor = foreColor;
            btn.Font = BodyBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.Height = 36;
        }

        /// <summary>
        /// Creates a modern metric card panel.
        /// </summary>
        public static Panel CreateMetricCard(string title, string value, Color accentColor, string subtext = "")
        {
            var card = new Panel
            {
                BackColor = ThemeColors.CardBg,
                Size = new Size(190, 100),
                Margin = new Padding(8),
                Padding = new Padding(12)
            };

            // Top accent color bar
            var accentBar = new Panel
            {
                BackColor = accentColor,
                Dock = DockStyle.Top,
                Height = 4
            };

            var lblTitle = new Label
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = ThemeColors.TextSecondary,
                Location = new Point(12, 12),
                AutoSize = true
            };

            var lblValue = new Label
            {
                Text = value,
                Font = StatNumberFont,
                ForeColor = accentColor,
                Location = new Point(10, 32),
                AutoSize = true
            };

            var lblSub = new Label
            {
                Text = subtext,
                Font = SmallFont,
                ForeColor = ThemeColors.TextMuted,
                Location = new Point(12, 72),
                AutoSize = true
            };

            card.Controls.Add(lblSub);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);
            card.Controls.Add(accentBar);

            // Paint light border
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(ThemeColors.BorderColor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            return card;
        }

        // MessageBox Wrappers
        public static void ShowSuccess(string message, string title = "NeighbourHub - Success")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowError(string message, string title = "NeighbourHub - Error")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowWarning(string message, string title = "NeighbourHub - Warning")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool Confirm(string message, string title = "NeighbourHub - Confirmation")
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}
