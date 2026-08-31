using System;

namespace NeighbourHub.Models
{
    public class Visitor
    {
        public int VisitorId { get; set; }
        public int FlatId { get; set; }
        public int ResidentUserId { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Purpose { get; set; }
        public DateTime CheckInTime { get; set; } = DateTime.Now;
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } = "Inside"; // 'Inside', 'Checked Out', 'Expected'
    }
}
