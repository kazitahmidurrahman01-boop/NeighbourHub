using System;

namespace NeighbourHub.Models
{
    public class SharedItem
    {
        public int ItemId { get; set; }
        public int OwnerUserId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = "Tools"; // 'Tools', 'Appliances', 'Ladders', 'Sports/Games', 'Books', 'Other'
        public string? Description { get; set; }
        public string AvailabilityStatus { get; set; } = "Available"; // 'Available', 'Borrowed', 'Unavailable'
        public string? ContactNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
