using System;

namespace NeighbourHub.Models
{
    public class EmergencyContact
    {
        public int ContactId { get; set; }
        public int? BuildingId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? AltPhone { get; set; }
        public string AvailableHours { get; set; } = "24/7";
        public string? Address { get; set; }
    }
}
