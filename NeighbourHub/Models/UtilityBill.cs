using System;

namespace NeighbourHub.Models
{
    public class UtilityBill
    {
        public int UtilityId { get; set; }
        public int BuildingId { get; set; }
        public int? FlatId { get; set; }
        public string UtilityType { get; set; } = "Electricity"; // 'Electricity', 'Water', 'Gas', 'Internet', 'Waste', 'Generator'
        public string BillingMonth { get; set; } = string.Empty;
        public int BillingYear { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(10);
        public string Status { get; set; } = "Unpaid"; // 'Unpaid', 'Paid'
    }
}
