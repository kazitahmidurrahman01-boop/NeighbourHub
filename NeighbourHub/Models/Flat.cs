using System;

namespace NeighbourHub.Models
{
    public class Flat
    {
        public int FlatId { get; set; }
        public int BuildingId { get; set; }
        public string FlatNumber { get; set; } = string.Empty;
        public int FloorNumber { get; set; }
        public int Bedrooms { get; set; } = 2;
        public int Bathrooms { get; set; } = 2;
        public decimal AreaSqFt { get; set; } = 1000;
        public decimal MonthlyRent { get; set; }
        public string Status { get; set; } = "Vacant"; // 'Vacant', 'Occupied', 'Under Maintenance'
        public string? Description { get; set; }
    }
}
