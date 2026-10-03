using System.ComponentModel.DataAnnotations;

namespace NEXUS_eProject.Models
{
    public class CustomerApplyViewModel
    {
        [Required]
        [Display(Name = "Connection Type")]
        public string ConnectionType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Internet / Telephone Plan")]
        public int PlanId { get; set; }

        [StringLength(150)]
        [Display(Name = "Equipment")]
        public string? Equipment { get; set; }

        [StringLength(1000)]
        [Display(Name = "Additional Information")]
        public string? AdditionalInfo { get; set; }
    }
}