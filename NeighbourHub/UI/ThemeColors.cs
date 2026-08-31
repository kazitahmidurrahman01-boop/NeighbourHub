using System.Drawing;

namespace NeighbourHub.UI
{
    /// <summary>
    /// Color Palette and Theme Constants for NeighbourHub.
    /// Modern Blue, Navy, White, and Emerald theme.
    /// </summary>
    public static class ThemeColors
    {
        // Dark Sidebar & Header
        public static readonly Color SidebarDark = Color.FromArgb(15, 23, 42);       // Slate 900
        public static readonly Color SidebarHover = Color.FromArgb(30, 41, 59);      // Slate 800
        public static readonly Color SidebarActive = Color.FromArgb(37, 99, 235);     // Blue 600
        public static readonly Color HeaderDark = Color.FromArgb(30, 41, 59);        // Slate 800

        // Brand Accents
        public static readonly Color Primary = Color.FromArgb(37, 99, 235);          // Royal Blue
        public static readonly Color PrimaryDark = Color.FromArgb(29, 78, 216);      // Blue 700
        public static readonly Color PrimaryLight = Color.FromArgb(219, 234, 254);   // Blue 100
        public static readonly Color Secondary = Color.FromArgb(71, 85, 105);       // Slate 600

        // Status & Alerts
        public static readonly Color Success = Color.FromArgb(16, 185, 129);        // Emerald
        public static readonly Color Warning = Color.FromArgb(245, 158, 11);        // Amber
        public static readonly Color Danger = Color.FromArgb(239, 68, 68);          // Rose
        public static readonly Color Info = Color.FromArgb(6, 182, 212);            // Cyan
        public static readonly Color Purple = Color.FromArgb(139, 92, 246);         // Violet

        // Backgrounds & Surfaces
        public static readonly Color Background = Color.FromArgb(248, 250, 252);     // Slate 50
        public static readonly Color CardBg = Color.White;
        public static readonly Color BorderColor = Color.FromArgb(226, 232, 240);    // Slate 200

        // Typography
        public static readonly Color TextPrimary = Color.FromArgb(30, 41, 59);      // Slate 800
        public static readonly Color TextSecondary = Color.FromArgb(100, 116, 139); // Slate 500
        public static readonly Color TextMuted = Color.FromArgb(148, 163, 184);     // Slate 400
        public static readonly Color TextLight = Color.FromArgb(248, 250, 252);     // Slate 50

        // Grid
        public static readonly Color GridRowAlternate = Color.FromArgb(248, 250, 252);
        public static readonly Color GridHeaderBg = Color.FromArgb(241, 245, 249);
    }
}
