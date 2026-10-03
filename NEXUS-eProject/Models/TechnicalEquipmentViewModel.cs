namespace NEXUS_eProject.Models
{
    public class TechnicalEquipmentViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string VendorName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool InStock => Quantity > 0;
    }
}