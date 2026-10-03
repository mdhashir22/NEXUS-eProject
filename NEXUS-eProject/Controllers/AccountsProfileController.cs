using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Accounts Employee")]
    public class AccountsProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountsProfileController(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        // =====================================================
        // ACCOUNTS EMPLOYEE PROFILE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }


            var email =
                user.Email ??
                user.UserName ??
                string.Empty;


            // Match Identity account with Employee record.
            var employee =
                await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e =>
                        e.Email.ToLower() ==
                        email.ToLower());


            var roles =
                await _userManager.GetRolesAsync(user);


            var role =
                roles.FirstOrDefault() ??
                "Accounts Employee";


            string fullName =
                employee?.FullName ??
                user.UserName ??
                email;


            string initial = "A";

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                initial =
                    fullName.Trim()
                        .Substring(0, 1)
                        .ToUpper();
            }


            var model =
                new AccountsProfileViewModel
                {
                    UserId =
                        user.Id,

                    EmployeeId =
                        employee?.EmployeeId,

                    FullName =
                        fullName,

                    Email =
                        user.Email ??
                        employee?.Email ??
                        "-",

                    UserName =
                        user.UserName ??
                        "-",

                    PhoneNumber =
                        employee?.Phone ??
                        user.PhoneNumber ??
                        "-",

                    Address =
                        employee?.Address ??
                        "-",

                    Department =
                        employee?.Department ??
                        "Accounts",

                    Role =
                        role,

                    IsActive =
                        employee?.IsActive ??
                        true,

                    Initial =
                        initial
                };


            return View(model);
        }
    }
}