using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EmployeesController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =====================================================
        // EMPLOYEE LIST
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .OrderByDescending(e => e.EmployeeId)
                .ToListAsync();

            return View(employees);
        }


        // =====================================================
        // CREATE EMPLOYEE - GET
        // =====================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // =====================================================
        // CREATE EMPLOYEE - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            EmployeeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // -------------------------------------------------
            // CHECK EMAIL IN IDENTITY
            // -------------------------------------------------

            var existingUser =
                await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists.");

                return View(model);
            }

            // -------------------------------------------------
            // CHECK ROLE
            // -------------------------------------------------

            string[] allowedRoles =
            {
                "Retail Employee",
                "Technical Employee",
                "Accounts Employee"
            };

            if (!allowedRoles.Contains(model.Role))
            {
                ModelState.AddModelError(
                    "Role",
                    "Please select a valid employee role.");

                return View(model);
            }

            // -------------------------------------------------
            // CREATE IDENTITY USER
            // -------------------------------------------------

            var identityUser = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var userResult =
                await _userManager.CreateAsync(
                    identityUser,
                    model.Password);

            if (!userResult.Succeeded)
            {
                foreach (var error in userResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            // -------------------------------------------------
            // ASSIGN ROLE
            // -------------------------------------------------

            var roleResult =
                await _userManager.AddToRoleAsync(
                    identityUser,
                    model.Role);

            if (!roleResult.Succeeded)
            {
                // Remove Identity account if role assignment fails
                await _userManager.DeleteAsync(identityUser);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            // -------------------------------------------------
            // CREATE EMPLOYEE PROFILE
            // -------------------------------------------------

            var employee = new Employee
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                Role = model.Role,
                Department = model.Department,
                IsActive = true,
                CreatedAt = DateTime.Now,
                IdentityUserId = identityUser.Id
            };

            _context.Employees.Add(employee);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Employee account created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        // =====================================================
        // EDIT EMPLOYEE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee =
                await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }


        // =====================================================
        // EDIT EMPLOYEE - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(employee);
            }

            try
            {
                var existingEmployee =
                    await _context.Employees
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            e => e.EmployeeId == id);

                if (existingEmployee == null)
                {
                    return NotFound();
                }

                employee.IdentityUserId =
                    existingEmployee.IdentityUserId;

                employee.CreatedAt =
                    existingEmployee.CreatedAt;

                _context.Update(employee);

                // ---------------------------------------------
                // UPDATE IDENTITY EMAIL
                // ---------------------------------------------

                if (!string.IsNullOrWhiteSpace(
                    employee.IdentityUserId))
                {
                    var identityUser =
                        await _userManager.FindByIdAsync(
                            employee.IdentityUserId);

                    if (identityUser != null)
                    {
                        identityUser.Email = employee.Email;
                        identityUser.UserName = employee.Email;

                        await _userManager.UpdateAsync(
                            identityUser);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Employee updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.EmployeeId))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // DELETE EMPLOYEE - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee =
                await _context.Employees
                    .FirstOrDefaultAsync(
                        e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }


        // =====================================================
        // DELETE EMPLOYEE - POST
        // =====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var employee =
                await _context.Employees.FindAsync(id);

            if (employee != null)
            {
                // ---------------------------------------------
                // DELETE IDENTITY ACCOUNT
                // ---------------------------------------------

                if (!string.IsNullOrWhiteSpace(
                    employee.IdentityUserId))
                {
                    var identityUser =
                        await _userManager.FindByIdAsync(
                            employee.IdentityUserId);

                    if (identityUser != null)
                    {
                        await _userManager.DeleteAsync(
                            identityUser);
                    }
                }

                // ---------------------------------------------
                // DELETE EMPLOYEE PROFILE
                // ---------------------------------------------

                _context.Employees.Remove(employee);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Employee deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // EMPLOYEE EXISTS
        // =====================================================

        private bool EmployeeExists(int id)
        {
            return _context.Employees
                .Any(e => e.EmployeeId == id);
        }
    }
}