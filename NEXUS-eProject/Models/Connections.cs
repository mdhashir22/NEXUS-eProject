using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NEXUS_eProject.Models
{
    public class Connection
    {
        [Key]
        public int ConnectionId { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountId { get; set; } = string.Empty;

        [Required]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string ConnectionType { get; set; } = string.Empty;

        [Required]
        public int PlanId { get; set; }

        [ForeignKey(nameof(PlanId))]
        public Plan Plan { get; set; } = null!;

        [StringLength(150)]
        public string? Equipment { get; set; }

        [StringLength(100)]
        public string? RouterSerial { get; set; }

        public DateTime? InstallationDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ActivatedAt { get; set; }
    }
}