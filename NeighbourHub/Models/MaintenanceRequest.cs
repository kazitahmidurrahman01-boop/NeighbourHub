using System;

namespace NeighbourHub.Models
{
    public class MaintenanceRequest
    {
        public int MaintenanceId { get; set; }
        public int BuildingId { get; set; }
        public int? FlatId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public string? VendorName { get; set; }
        public DateTime ScheduledDate { get; set; } = DateTime.Today;
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; } = "Scheduled"; // 'Scheduled', 'In Progress', 'Completed', 'Cancelled'
    }
}
