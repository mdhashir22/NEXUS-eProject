using System.ComponentModel.DataAnnotations;

namespace NEXUS_eProject.Models
{
    public class AiChatRequest
    {
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
    }
}