using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NEXUS_eProject.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderNumber { get; set; } = string.Empty;

        // Customer who placed the order
        [Required]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;

        // Requested connection
        [Required]
        [StringLength(20)]
        public string ConnectionType { get; set; } = string.Empty;

        [Required]
        public int PlanId { get; set; }

        [ForeignKey(nameof(PlanId))]
        public Plan Plan { get; set; } = null!;

        // Equipment requested for the connection
        [StringLength(150)]
        public string? Equipment { get; set; }

        // Current order stage
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }
    }
}