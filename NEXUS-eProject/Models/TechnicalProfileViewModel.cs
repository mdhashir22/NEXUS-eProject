namespace NEXUS_eProject.Models
{
    public class TechnicalProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public int? EmployeeId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Role { get; set; } = "Technical Employee";

        public bool IsActive { get; set; }

        public string Initial { get; set; } = "T";
    }
}