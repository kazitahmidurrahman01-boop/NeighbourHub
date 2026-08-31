using System;

namespace NeighbourHub.Models
{
    public class Building
    {
        public int BuildingId { get; set; }
        public int PropertyId { get; set; }
        public int? ManagerUserId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public string? BuildingCode { get; set; }
        public int TotalUnits { get; set; }
        public string? Address { get; set; }
        public string? SecurityContact { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
