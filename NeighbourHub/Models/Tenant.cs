using System;

namespace NeighbourHub.Models
{
    public class Tenant
    {
        public int TenantId { get; set; }
        public int UserId { get; set; }
        public int FlatId { get; set; }
        public DateTime LeaseStartDate { get; set; }
        public DateTime? LeaseEndDate { get; set; }
        public decimal AgreedRent { get; set; }
        public decimal SecurityDeposit { get; set; }
        public string? EmergencyContact { get; set; }
        public string Status { get; set; } = "Active"; // 'Active', 'Terminated'
    }
}
