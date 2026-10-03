using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Accounts Employee")]
    public class AccountsConnectionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsConnectionController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // ALL CONNECTIONS - READ ONLY
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status)
        {
            var query = _context.Connections
                .AsNoTracking()
                .Include(c => c.Customer)
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.AccountId.Contains(search) ||

                    (c.Customer != null &&
                     c.Customer.FullName.Contains(search)) ||

                    (c.Customer != null &&
                     c.Customer.Phone.Contains(search)) ||

                    (c.Order != null &&
                     c.Order.OrderNumber.Contains(search)));
            }


            if (!string.IsNullOrWhiteSpace(status) &&
                status != "All")
            {
                query = query.Where(c =>
                    c.Status == status);
            }


            var connections = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();


            ViewBag.Search = search;
            ViewBag.Status = status;


            return View(connections);
        }


        // =====================================================
        // CONNECTION DETAILS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var connection = await _context.Connections
                .AsNoTracking()
                .Include(c => c.Customer)
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c =>
                    c.ConnectionId == id);


            if (connection == null)
            {
                return NotFound();
            }


            // =================================================
            // BILLS
            // =================================================

            var bills = await _context.Bills
                .AsNoTracking()
                .Where(b =>
                    b.ConnectionId == id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();


            // =================================================
            // PAYMENTS FOR CONNECTION BILLS
            // =================================================

            var payments = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Bill)
                .Where(p =>
                    p.Bill != null &&
                    p.Bill.ConnectionId == id)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();


            // =================================================
            // FINANCIAL SUMMARY
            // =================================================

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


            return View(connection);
        }
    }
}