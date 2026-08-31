using System;

namespace NeighbourHub.Models
{
    public class Complaint
    {
        public int ComplaintId { get; set; }
        public int UserId { get; set; }
        public int? FlatId { get; set; }
        public int BuildingId { get; set; }
        public string Category { get; set; } = "Other"; // 'Plumbing', 'Electrical', 'Elevator', 'Security', 'Cleanliness', 'Noise', 'Other'
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium"; // 'Low', 'Medium', 'High', 'Urgent'
        public string Status { get; set; } = "Pending"; // 'Pending', 'In Progress', 'Solved', 'Rejected'
        public int? AssignedToManagerId { get; set; }
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public DateTime? ResolvedDate { get; set; }
        public string? ResolutionNotes { get; set; }
    }
}
