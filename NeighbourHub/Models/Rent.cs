using System;

namespace NeighbourHub.Models
{
    public class Rent
    {
        public int RentId { get; set; }
        public int FlatId { get; set; }
        public int TenantId { get; set; }
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal RentAmount { get; set; }
        public decimal UtilityCharges { get; set; }
        public decimal TotalAmount => RentAmount + UtilityCharges;
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Due"; // 'Paid', 'Due', 'Partially Paid'
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
