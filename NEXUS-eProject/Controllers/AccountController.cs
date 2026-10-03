using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }


        // =====================================================
        // LOGIN - GET
        // =====================================================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }


        // =====================================================
        // LOGIN - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;


            // -------------------------------------------------
            // VALIDATE MODEL
            // -------------------------------------------------

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // -------------------------------------------------
            // FIND USER
            // -------------------------------------------------

            var user =
                await _userManager.FindByEmailAsync(
                    model.Email);


            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }


            // -------------------------------------------------
            // PASSWORD LOGIN
            // -------------------------------------------------

            var result =
                await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);


            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }


            // =================================================
            // GET USER ROLES
            // =================================================

            var roles =
                await _userManager.GetRolesAsync(user);


            // =================================================
            // ADMIN
            // =================================================

            if (roles.Contains("Admin"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin");
            }


            // =================================================
            // RETAIL EMPLOYEE
            // =================================================

            if (roles.Contains("Retail Employee"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Retail");
            }


            // =================================================
            // TECHNICAL EMPLOYEE
            // =================================================

            if (roles.Contains("Technical Employee"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Technical");
            }


            // =================================================
            // ACCOUNTS EMPLOYEE
            // =================================================

            if (roles.Contains("Accounts Employee"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Accounts");
            }


            // =================================================
            // CUSTOMER
            // =================================================

            if (roles.Contains("Customer"))
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }


            // =================================================
            // UNKNOWN / INVALID ROLE
            // =================================================

            await _signInManager.SignOutAsync();


            ModelState.AddModelError(
                string.Empty,
                "Your account does not have a valid role.");


            return View(model);
        }


        // =====================================================
        // REGISTER - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            // =================================================
            // CHECK ADMIN REGISTRATION SETTING
            // =================================================

            var settings =
                await GetSystemSettingsAsync();


            if (!settings.CustomerRegistrationEnabled)
            {
                TempData["RegistrationDisabled"] =
                    "New customer registration is currently unavailable. Please contact NEXUS support for assistance.";

                return RedirectToAction(
                    "Index",
                    "Home");
            }


            return View();
        }


        // =====================================================
        // REGISTER - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            CustomerRegisterViewModel model)
        {
            // =================================================
            // CHECK ADMIN REGISTRATION SETTING AGAIN
            // =================================================

            /*
             * This check is intentionally performed on POST too.
             *
             * Even if someone manually submits the registration
             * form while registration is disabled, the server
             * will refuse to create the account.
             */
            var settings =
                await GetSystemSettingsAsync();


            if (!settings.CustomerRegistrationEnabled)
            {
                TempData["RegistrationDisabled"] =
                    "New customer registration is currently unavailable. Please contact NEXUS support for assistance.";

                return RedirectToAction(
                    "Index",
                    "Home");
            }


            // -------------------------------------------------
            // VALIDATE MODEL
            // -------------------------------------------------

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // -------------------------------------------------
            // CHECK EXISTING ACCOUNT
            // -------------------------------------------------

            var existingUser =
                await _userManager.FindByEmailAsync(
                    model.Email);


            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists.");

                return View(model);
            }


            // =================================================
            // CREATE IDENTITY USER
            // =================================================

            var identityUser =
                new IdentityUser
                {
                    UserName = model.Email,

                    Email = model.Email,

                    EmailConfirmed = true
                };


            var createResult =
                await _userManager.CreateAsync(
                    identityUser,
                    model.Password);


            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }


                return View(model);
            }


            // =================================================
            // ASSIGN CUSTOMER ROLE
            // =================================================

            var roleResult =
                await _userManager.AddToRoleAsync(
                    identityUser,
                    "Customer");


            if (!roleResult.Succeeded)
            {
                // ---------------------------------------------
                // ROLLBACK IDENTITY USER
                // ---------------------------------------------

                await _userManager.DeleteAsync(
                    identityUser);


                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }


                return View(model);
            }


            // =================================================
            // CREATE CUSTOMER RECORD
            // =================================================

            var customer =
                new Customer
                {
                    FullName =
                        model.FullName,

                    FatherName =
                        model.FatherName,

                    Phone =
                        model.Phone,

                    Email =
                        model.Email,

                    Address =
                        model.Address,

                    City =
                        model.City,

                    IsActive =
                        true,

                    CreatedAt =
                        DateTime.Now,

                    IdentityUserId =
                        identityUser.Id
                };


            _context.Customers.Add(customer);


            // =================================================
            // SAVE CUSTOMER
            // =================================================

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                /*
                 * Identity user has already been created.
                 * If Customer table saving fails, remove that
                 * Identity user so we don't leave an orphan
                 * account behind.
                 */

                await _userManager.DeleteAsync(
                    identityUser);


                ModelState.AddModelError(
                    string.Empty,
                    "Your customer account could not be created. Please try again.");


                return View(model);
            }


            // =================================================
            // AUTOMATICALLY LOGIN CUSTOMER
            // =================================================

            await _signInManager.SignInAsync(
                identityUser,
                isPersistent: false);


            // -------------------------------------------------
            // REDIRECT CUSTOMER
            // -------------------------------------------------

            return RedirectToAction(
                "Index",
                "Home");
        }


        // =====================================================
        // LOGOUT
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();


            return RedirectToAction(
                "Index",
                "Home");
        }


        // =====================================================
        // ACCESS DENIED
        // =====================================================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // =====================================================
        // PRIVATE - GET SYSTEM SETTINGS
        // =====================================================

        private async Task<SystemSetting>
            GetSystemSettingsAsync()
        {
            var settings =
                await _context.SystemSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync();


            /*
             * SAFE FALLBACK
             *
             * If the SystemSettings row is missing for any
             * unexpected reason, registration remains enabled
             * so the public website does not break.
             */
            if (settings == null)
            {
                return new SystemSetting
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

                    MaintenanceMode = false
                };
            }


            return settings;
        }
    }
}