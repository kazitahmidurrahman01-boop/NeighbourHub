using System;

namespace NeighbourHub.Models
{
    public class Notice
    {
        public int NoticeId { get; set; }
        public int BuildingId { get; set; }
        public int PublishedByUserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = "General"; // 'General', 'Maintenance', 'Emergency', 'Meeting', 'Billing'
        public string Priority { get; set; } = "Normal"; // 'Normal', 'Important', 'Urgent'
        public DateTime PublishedDate { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
