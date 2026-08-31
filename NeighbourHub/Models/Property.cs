using System;

namespace NeighbourHub.Models
{
    public class Property
    {
        public int PropertyId { get; set; }
        public int OwnerUserId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyType { get; set; } = "Residential Complex";
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = "Dhaka";
        public int TotalFloors { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
