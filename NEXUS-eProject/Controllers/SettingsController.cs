using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // SETTINGS PAGE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = await GetOrCreateSettingsAsync();

            return View(settings);
        }


        // =====================================================
        // SAVE SETTINGS
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(SystemSetting model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Some settings could not be saved. Please check the highlighted fields.";

                return View("Index", model);
            }


            var settings = await _context.SystemSettings
                .FirstOrDefaultAsync();


            // If settings somehow do not exist yet,
            // create the first settings record.
            if (settings == null)
            {
                settings = new SystemSetting();

                _context.SystemSettings.Add(settings);
            }


            // =================================================
            // GENERAL SETTINGS
            // =================================================

            settings.ApplicationName =
                model.ApplicationName.Trim();

            settings.SupportEmail =
                string.IsNullOrWhiteSpace(model.SupportEmail)
                    ? ""
                    : model.SupportEmail.Trim();

            settings.SupportPhone =
                string.IsNullOrWhiteSpace(model.SupportPhone)
                    ? ""
                    : model.SupportPhone.Trim();

            settings.CompanyAddress =
                string.IsNullOrWhiteSpace(model.CompanyAddress)
                    ? ""
                    : model.CompanyAddress.Trim();


            // =================================================
            // BILLING SETTINGS
            // =================================================

            settings.TaxPercentage =
                model.TaxPercentage;

            settings.DefaultBillDueDays =
                model.DefaultBillDueDays;

            settings.Currency =
                model.Currency.Trim().ToUpper();


            // =================================================
            // CONNECTION SETTINGS
            // =================================================

            settings.CityCode =
                model.CityCode.Trim();

            settings.BroadbandPrefix =
                model.BroadbandPrefix.Trim().ToUpper();

            settings.TelephonePrefix =
                model.TelephonePrefix.Trim().ToUpper();

            settings.DialUpPrefix =
                model.DialUpPrefix.Trim().ToUpper();


            // =================================================
            // SYSTEM CONTROLS
            // =================================================

            settings.CustomerRegistrationEnabled =
                model.CustomerRegistrationEnabled;

            settings.FeedbackEnabled =
                model.FeedbackEnabled;

            settings.MaintenanceMode =
                model.MaintenanceMode;


            // =================================================
            // AUDIT INFORMATION
            // =================================================

            settings.UpdatedAt =
                DateTime.Now;

            settings.UpdatedBy =
                User.Identity?.Name ?? "Administrator";


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "NEXUS system settings have been updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // RESET SETTINGS TO DEFAULTS
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset()
        {
            var settings = await _context.SystemSettings
                .FirstOrDefaultAsync();


            if (settings == null)
            {
                settings = new SystemSetting();

                _context.SystemSettings.Add(settings);
            }


            // =================================================
            // GENERAL DEFAULTS
            // =================================================

            settings.ApplicationName = "NEXUS";

            settings.SupportEmail = "support@nexus.com";

            settings.SupportPhone = "";

            settings.CompanyAddress = "";


            // =================================================
            // BILLING DEFAULTS
            // =================================================

            settings.TaxPercentage = 12.24m;

            settings.DefaultBillDueDays = 10;

            settings.Currency = "PKR";


            // =================================================
            // CONNECTION DEFAULTS
            // =================================================

            settings.CityCode = "101";

            settings.BroadbandPrefix = "B";

            settings.TelephonePrefix = "T";

            settings.DialUpPrefix = "D";


            // =================================================
            // SYSTEM CONTROL DEFAULTS
            // =================================================

            settings.CustomerRegistrationEnabled = true;

            settings.FeedbackEnabled = true;

            settings.MaintenanceMode = false;


            // =================================================
            // AUDIT
            // =================================================

            settings.UpdatedAt = DateTime.Now;

            settings.UpdatedBy =
                User.Identity?.Name ?? "Administrator";


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "NEXUS settings have been restored to their default values.";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // PRIVATE: GET OR CREATE SETTINGS
        // =====================================================

        private async Task<SystemSetting> GetOrCreateSettingsAsync()
        {
            var settings = await _context.SystemSettings
                .FirstOrDefaultAsync();


            if (settings != null)
            {
                return settings;
            }


            settings = new SystemSetting
            {
                ApplicationName = "NEXUS",

                SupportEmail = "support@nexus.com",

                SupportPhone = "",

                CompanyAddress = "",

                TaxPercentage = 12.24m,

                DefaultBillDueDays = 10,

                Currency = "PKR",

                CityCode = "101",

                BroadbandPrefix = "B",

                TelephonePrefix = "T",

                DialUpPrefix = "D",

                CustomerRegistrationEnabled = true,

                FeedbackEnabled = true,

                MaintenanceMode = false,

                UpdatedAt = DateTime.Now,

                UpdatedBy =
                    User.Identity?.Name ?? "Administrator"
            };


            _context.SystemSettings.Add(settings);

            await _context.SaveChangesAsync();


            return settings;
        }
    }
}