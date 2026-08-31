using System;

namespace NeighbourHub.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int? RentId { get; set; }
        public int TenantId { get; set; }
        public int FlatId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string PaymentMethod { get; set; } = "Cash"; // 'Cash', 'Bank Transfer', 'bKash/Nagad', 'Card'
        public string? TransactionRef { get; set; }
        public string PaidBy { get; set; } = string.Empty;
        public string Status { get; set; } = "Completed";
        public string? Remarks { get; set; }
    }
}
