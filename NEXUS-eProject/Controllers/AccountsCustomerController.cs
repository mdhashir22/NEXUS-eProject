using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Accounts Employee")]
    public class AccountsCustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsCustomerController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // CUSTOMER LIST
        // READ ONLY
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
                    (c.Email != null && c.Email.Contains(search)) ||
                    (c.Phone != null && c.Phone.Contains(search)) ||
                    c.City.Contains(search));
            }


            // STATUS
            if (!string.IsNullOrWhiteSpace(status) &&
                status != "All")
            {
                if (status == "Active")
                {
                    query = query.Where(c =>
                        c.IsActive);
                }
                else if (status == "Inactive")
                {
                    query = query.Where(c =>
                        !c.IsActive);
                }
            }


            var customers = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();


            ViewBag.Search = search;
            ViewBag.Status = status;


            return View(customers);
        }


        // =====================================================
        // CUSTOMER DETAILS
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


            // ---------------------------------------------
            // CONNECTIONS
            // ---------------------------------------------

            var connections = await _context.Connections
                .AsNoTracking()
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .Where(c =>
                    c.CustomerId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();


            // ---------------------------------------------
            // BILLS
            // ---------------------------------------------

            var bills = await _context.Bills
                .AsNoTracking()
                .Where(b =>
                    b.CustomerId == id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();


            // ---------------------------------------------
            // PAYMENTS
            // ---------------------------------------------

            var payments = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Bill)
                .Where(p =>
                    p.CustomerId == id)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();


            // ---------------------------------------------
            // FINANCIAL SUMMARY
            // ---------------------------------------------

            var totalBilled =
                bills.Sum(b =>
                    b.TotalAmount);


            var totalPaid =
                payments
                    .Where(p =>
                        p.Status == "Paid")
                    .Sum(p =>
                        p.Amount);


            var outstanding =
                totalBilled -
                totalPaid;


            if (outstanding < 0)
            {
                outstanding = 0;
            }


            // ---------------------------------------------
            // VIEW DATA
            // ---------------------------------------------

            ViewBag.Connections =
                connections;

            ViewBag.Bills =
                bills;

            ViewBag.Payments =
                payments;

            ViewBag.TotalBilled =
                totalBilled;

            ViewBag.TotalPaid =
                totalPaid;

            ViewBag.Outstanding =
                outstanding;


            return View(customer);
        }
    }
}