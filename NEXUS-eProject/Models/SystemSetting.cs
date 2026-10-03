using System.ComponentModel.DataAnnotations;

namespace NEXUS_eProject.Models
{
    public class SystemSetting
    {
        // =====================================================
        // PRIMARY KEY
        // =====================================================

        [Key]
        public int SystemSettingId { get; set; }


        // =====================================================
        // GENERAL SETTINGS
        // =====================================================

        [Required]
        [StringLength(100)]
        public string ApplicationName { get; set; } = "NEXUS";


        [StringLength(150)]
        [EmailAddress]
        public string? SupportEmail { get; set; } = "support@nexus.com";


        [StringLength(30)]
        public string? SupportPhone { get; set; }


        [StringLength(300)]
        public string? CompanyAddress { get; set; }


        // =====================================================
        // BILLING SETTINGS
        // =====================================================

        [Range(0, 100)]
        public decimal TaxPercentage { get; set; } = 12.24m;


        [Range(1, 365)]
        public int DefaultBillDueDays { get; set; } = 10;


        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "PKR";


        // =====================================================
        // CONNECTION SETTINGS
        // =====================================================

        [Required]
        [StringLength(10)]
        public string CityCode { get; set; } = "101";


        [Required]
        [StringLength(5)]
        public string BroadbandPrefix { get; set; } = "B";


        [Required]
        [StringLength(5)]
        public string TelephonePrefix { get; set; } = "T";


        [Required]
        [StringLength(5)]
        public string DialUpPrefix { get; set; } = "D";


        // =====================================================
        // SYSTEM CONTROLS
        // =====================================================

        public bool CustomerRegistrationEnabled { get; set; } = true;


        public bool FeedbackEnabled { get; set; } = true;


        public bool MaintenanceMode { get; set; } = false;


        // =====================================================
        // AUDIT INFORMATION
        // =====================================================

        public DateTime UpdatedAt { get; set; } = DateTime.Now;


        [StringLength(150)]
        public string? UpdatedBy { get; set; }
    }
}