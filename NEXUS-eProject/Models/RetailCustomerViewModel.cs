using System.ComponentModel.DataAnnotations;

namespace NEXUS_eProject.Models
{
    public class RetailCustomerViewModel
    {
        // =====================================================
        // CUSTOMER INFORMATION
        // =====================================================

        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Father name is required.")]
        [StringLength(100, ErrorMessage = "Father name cannot exceed 100 characters.")]
        [Display(Name = "Father Name")]
        public string FatherName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;


        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string? Email { get; set; }


        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        public string Address { get; set; } = string.Empty;


        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string City { get; set; } = string.Empty;


        // =====================================================
        // STATUS
        // =====================================================

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;


        // =====================================================
        // SYSTEM INFORMATION
        // =====================================================

        public DateTime CreatedAt { get; set; }


        // =====================================================
        // IDENTITY INFORMATION
        // =====================================================

        public string? IdentityUserId { get; set; }
    }
}