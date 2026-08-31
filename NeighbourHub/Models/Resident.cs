using System;

namespace NeighbourHub.Models
{
    public class Resident
    {
        public int ResidentId { get; set; }
        public int UserId { get; set; }
        public int FlatId { get; set; }
        public int? TenantId { get; set; }
        public string Relationship { get; set; } = "Self"; // 'Self', 'Spouse', 'Child', 'Parent', 'Other'
        public string? NID { get; set; }
        public string? Profession { get; set; }
        public DateTime MoveInDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active";
    }
}
