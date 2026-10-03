using System.ComponentModel.DataAnnotations;

namespace NEXUS_eProject.Models
{
    public class ContactFormViewModel
    {
        // =====================================================
        // CUSTOMER NAME
        // =====================================================

        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(
            100,
            ErrorMessage = "Full name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        // =====================================================
        // PHONE NUMBER
        // =====================================================

        [Required(ErrorMessage = "Please enter your phone number.")]
        [StringLength(
            30,
            ErrorMessage = "Phone number cannot exceed 30 characters.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;


        // =====================================================
        // EMAIL ADDRESS
        // =====================================================

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(
            150,
            ErrorMessage = "Email address cannot exceed 150 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;


        // =====================================================
        // SERVICE
        // =====================================================

        [StringLength(
            100,
            ErrorMessage = "Service cannot exceed 100 characters.")]
        [Display(Name = "Service")]
        public string? Service { get; set; }


        // =====================================================
        // MESSAGE
        // =====================================================

        [Required(ErrorMessage = "Please enter your message.")]
        [StringLength(
            2000,
            MinimumLength = 5,
            ErrorMessage = "Message must be between 5 and 2000 characters.")]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;
    }
}