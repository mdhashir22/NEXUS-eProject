using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NEXUS_eProject.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; } = null!;

        [Required]
        [StringLength(30)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Submitted";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? RespondedAt { get; set; }

        [StringLength(1000)]
        public string? AdminResponse { get; set; }
    }
}