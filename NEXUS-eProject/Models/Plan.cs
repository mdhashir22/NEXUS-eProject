using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NEXUS_eProject.Models
{
    public class Plan
    {
        [Key]
        public int PlanId { get; set; }

        [Required]
        [StringLength(50)]
        public string ConnectionType { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string PlanName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Speed { get; set; }

        [StringLength(50)]
        public string? Duration { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}