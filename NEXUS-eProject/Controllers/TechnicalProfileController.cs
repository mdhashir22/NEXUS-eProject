using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Technical Employee")]
    public class TechnicalProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TechnicalProfileController(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var identityUser = await _userManager.GetUserAsync(User);

            if (identityUser == null)
            {
                return Challenge();
            }

            var identityEmail =
                identityUser.Email?.Trim() ?? string.Empty;

            Employee? employee = null;

            if (!string.IsNullOrWhiteSpace(identityEmail))
            {
                var normalizedEmail = identityEmail.ToLower();

                employee = await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e =>
                        e.Email != null &&
                        e.Email.Trim().ToLower() == normalizedEmail);
            }

            // Fallback:
            // If for some reason Employee email does not match,
            // try Identity username as email.
            if (employee == null &&
                !string.IsNullOrWhiteSpace(identityUser.UserName))
            {
                var normalizedUserName =
                    identityUser.UserName.Trim().ToLower();

                employee = await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e =>
                        e.Email != null &&
                        e.Email.Trim().ToLower() == normalizedUserName);
            }

            var fullName =
                !string.IsNullOrWhiteSpace(employee?.FullName)
                    ? employee.FullName
                    : "Technical Employee";

            var phone =
                !string.IsNullOrWhiteSpace(employee?.Phone)
                    ? employee.Phone
                    : identityUser.PhoneNumber ?? string.Empty;

            var email =
                !string.IsNullOrWhiteSpace(employee?.Email)
                    ? employee.Email
                    : identityEmail;

            var model = new TechnicalProfileViewModel
            {
                UserId = identityUser.Id,

                EmployeeId = employee?.EmployeeId,

                FullName = fullName,

                Email = email,

                UserName =
                    identityUser.UserName ?? email,

                PhoneNumber = phone,

                Address =
                    employee?.Address ?? string.Empty,

                Department =
                    employee?.Department ?? string.Empty,

                Role =
                    !string.IsNullOrWhiteSpace(employee?.Role)
                        ? employee.Role
                        : "Technical Employee",

                IsActive =
                    employee?.IsActive ?? true,

                Initial =
                    !string.IsNullOrWhiteSpace(fullName)
                        ? fullName.Substring(0, 1).ToUpper()
                        : "T"
            };

            return View(model);
        }
    }
}