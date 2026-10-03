using System;
using System.ComponentModel.DataAnnotations;

namespace NEXUS_eProject.Models
{
    public class RetailApplicationViewModel
    {
        public int OrderId { get; set; }

        public string? OrderNumber { get; set; }

        public int CustomerId { get; set; }

        public string? CustomerName { get; set; }

        public string? FatherName { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        [Required]
        [Display(Name = "Connection Type")]
        public string ConnectionType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Plan")]
        public int PlanId { get; set; }

        public string? PlanName { get; set; }

        public string? PlanSpeed { get; set; }

        public decimal PlanPrice { get; set; }

        [Display(Name = "Equipment")]
        public string? Equipment { get; set; }

        [Display(Name = "Additional Information")]
        [StringLength(1000)]
        public string? AdditionalInfo { get; set; }

        public string Status { get; set; } = "Submitted";

        public string? RetailRemarks { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsAccepted { get; set; }

        public bool IsRejected { get; set; }
    }
}