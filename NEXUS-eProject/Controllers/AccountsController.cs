using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Accounts Employee")]
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var model = new AccountsDashboardViewModel
            {
                BillingPending =
                    await _context.Connections
                        .CountAsync(c =>
                            c.Order != null &&
                            (c.Order.Status == "Connection Created" ||
                             c.Order.Status == "Billing Pending")),

                TotalBills =
                    await _context.Bills.CountAsync(),

                UnpaidBills =
                    await _context.Bills
                        .CountAsync(b =>
                            b.Status == "Unpaid" ||
                            b.Status == "Partially Paid"),

                PaidBills =
                    await _context.Bills
                        .CountAsync(b => b.Status == "Paid"),

                PendingPayments =
                    await _context.Payments
                        .CountAsync(p =>
                            p.Status == "Pending" ||
                            p.Status == "Pending Verification"),

                VerifiedPayments =
                    await _context.Payments
                        .CountAsync(p => p.Status == "Paid"),

                OutstandingAmount =
                    await _context.Bills
                        .Where(b =>
                            b.Status == "Unpaid" ||
                            b.Status == "Partially Paid")
                        .SumAsync(b => (decimal?)b.TotalAmount) ?? 0m,

                TotalCollected =
                    await _context.Payments
                        .Where(p => p.Status == "Paid")
                        .SumAsync(p => (decimal?)p.Amount) ?? 0m,

                TodayCollection =
                    await _context.Payments
                        .Where(p =>
                            p.Status == "Paid" &&
                            p.PaymentDate >= today &&
                            p.PaymentDate < tomorrow)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0m,

                BillingPendingConnections =
                    await _context.Connections
                        .AsNoTracking()
                        .Include(c => c.Customer)
                        .Include(c => c.Plan)
                        .Include(c => c.Order)
                        .Where(c =>
                            c.Order != null &&
                            (c.Order.Status == "Connection Created" ||
                             c.Order.Status == "Billing Pending"))
                        .OrderByDescending(c => c.CreatedAt)
                        .Take(5)
                        .ToListAsync(),

                RecentBills =
                    await _context.Bills
                        .AsNoTracking()
                        .Include(b => b.Customer)
                        .Include(b => b.Connection)
                        .OrderByDescending(b => b.CreatedAt)
                        .Take(5)
                        .ToListAsync(),

                RecentPayments =
                    await _context.Payments
                        .AsNoTracking()
                        .Include(p => p.Customer)
                        .Include(p => p.Bill)
                        .OrderByDescending(p => p.PaymentDate)
                        .Take(5)
                        .ToListAsync()
            };

            return View(model);
        }
    }
}