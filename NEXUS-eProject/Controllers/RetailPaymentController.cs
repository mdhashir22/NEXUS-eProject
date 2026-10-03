using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Retail Employee")]
    public class RetailPaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RetailPaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? paymentMethod)
        {
            var query = _context.Payments
                .AsNoTracking()
                .Include(p => p.Customer)
                .Include(p => p.Bill)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.PaymentNumber.Contains(search) ||
                    p.AccountId.Contains(search) ||
                    (p.Customer != null &&
                     p.Customer.FullName.Contains(search)) ||
                    (p.Customer != null &&
                     p.Customer.Phone != null &&
                     p.Customer.Phone.Contains(search)));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p =>
                    p.Status == status);
            }

            // Payment method filter
            if (!string.IsNullOrWhiteSpace(paymentMethod))
            {
                query = query.Where(p =>
                    p.PaymentMethod == paymentMethod);
            }

            // Payments list
            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Statistics
            var allPayments = _context.Payments
                .AsNoTracking();

            ViewBag.TotalPayments =
                await allPayments.CountAsync();

            ViewBag.PaidPayments =
                await allPayments.CountAsync(p =>
                    p.Status == "Paid");

            ViewBag.PendingPayments =
                await allPayments.CountAsync(p =>
                    p.Status == "Pending" ||
                    p.Status == "Pending Verification");

            ViewBag.RejectedPayments =
                await allPayments.CountAsync(p =>
                    p.Status == "Rejected");

            // Total received amount
            ViewBag.TotalReceived =
                await allPayments
                    .Where(p => p.Status == "Paid")
                    .Select(p => (decimal?)p.Amount)
                    .SumAsync() ?? 0m;

            // Pending amount
            ViewBag.PendingAmount =
                await allPayments
                    .Where(p =>
                        p.Status == "Pending" ||
                        p.Status == "Pending Verification")
                    .Select(p => (decimal?)p.Amount)
                    .SumAsync() ?? 0m;

            // Preserve filters
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.PaymentMethod = paymentMethod;

            return View(payments);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Customer)
                .Include(p => p.Bill)
                    .ThenInclude(b => b.Connection)
                        .ThenInclude(c => c.Plan)
                .FirstOrDefaultAsync(p =>
                    p.PaymentId == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }
    }
}