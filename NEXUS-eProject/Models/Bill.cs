using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NEXUS_eProject.Models
{
    public class Bill
    {
        [Key]
        public int BillId { get; set; }

        [Required]
        [StringLength(30)]
        public string BillNumber { get; set; } = string.Empty;

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;

        [Required]
        public int ConnectionId { get; set; }

        [ForeignKey(nameof(ConnectionId))]
        public Connection Connection { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string AccountId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PlanAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EquipmentAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousDue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime BillingDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Unpaid";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}