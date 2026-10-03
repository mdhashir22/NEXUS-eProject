using System.ComponentModel.DataAnnotations;

namespace NEXUS_eProject.Models
{
    public class AccountsBillCreateViewModel
    {
        public int ConnectionId { get; set; }

        // Display-only data
        public string AccountId { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ConnectionType { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string Speed { get; set; } = string.Empty;
        public string Equipment { get; set; } = string.Empty;

        [Required]
        [Range(0, 999999999)]
        public decimal PlanAmount { get; set; }

        [Range(0, 999999999)]
        public decimal EquipmentAmount { get; set; }

        [Range(0, 999999999)]
        public decimal PreviousDue { get; set; }

        [Range(0, 999999999)]
        public decimal Discount { get; set; }

        [Range(0, 100)]
        public decimal TaxPercentage { get; set; } = 12.24m;

        [Required]
        [DataType(DataType.Date)]
        public DateTime BillingDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } =
            DateTime.Today.AddDays(10);
    }
}