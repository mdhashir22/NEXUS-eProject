using System.ComponentModel.DataAnnotations;

namespace NEXUS_eProject.Models
{
    public class EmployeeCreateViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;


        [StringLength(20)]
        public string? Phone { get; set; }


        [StringLength(250)]
        public string? Address { get; set; }


        [Required(ErrorMessage = "Please select a role.")]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;


        [StringLength(100)]
        public string? Department { get; set; }


        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please confirm the password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}