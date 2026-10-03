using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Retail Employee")]
    public class RetailCustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RetailCustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // CUSTOMER LIST
        // Retail Employee: View / Search only
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status)
        {
            var query = _context.Customers
                .AsNoTracking()
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.FullName.Contains(search) ||
                    c.FatherName.Contains(search) ||
                    c.Phone.Contains(search) ||
                    (c.Email != null &&
                     c.Email.Contains(search)) ||
                    c.City.Contains(search));
            }

            // STATUS FILTER
            if (status == "Active")
            {
                query = query.Where(c => c.IsActive);
            }
            else if (status == "Inactive")
            {
                query = query.Where(c => !c.IsActive);
            }

            var customers = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewData["Search"] = search;
            ViewData["Status"] = status;

            return View(customers);
        }


        // =====================================================
        // CUSTOMER DETAILS
        // Retail Employee: View only
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }
    }
}