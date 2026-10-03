using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Retail Employee")]
    public class RetailBillController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RetailBillController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // BILLS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? category)
        {
            var query = _context.Bills
                .AsNoTracking()
                .Include(b => b.Customer)
                .Include(b => b.Connection)
                    .ThenInclude(c => c.Plan)
                .AsQueryable();

            // -----------------------------------------------------
            // SEARCH
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(b =>
                    b.BillNumber.Contains(search) ||
                    b.AccountId.Contains(search) ||
                    (b.Customer != null &&
                     b.Customer.FullName.Contains(search)) ||
                    (b.Customer != null &&
                     b.Customer.Phone != null &&
                     b.Customer.Phone.Contains(search)));
            }

            // -----------------------------------------------------
            // STATUS FILTER
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(b => b.Status == status);
            }

            // -----------------------------------------------------
            // CATEGORY FILTER
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(category))
            {
                var today = DateTime.Today;

                switch (category)
                {
                    case "Paid":

                        query = query.Where(b =>
                            b.Status == "Paid");

                        break;

                    case "Unpaid":

                        query = query.Where(b =>
                            b.Status == "Unpaid");

                        break;

                    case "Partial":

                        query = query.Where(b =>
                            b.Status == "Partially Paid" ||
                            b.Status == "Partial");

                        break;

                    case "Overdue":

                        query = query.Where(b =>
                            b.DueDate < today &&
                            b.Status != "Paid");

                        break;
                }
            }

            var bills = await query
                .OrderByDescending(b => b.BillingDate)
                .ThenByDescending(b => b.CreatedAt)
                .ToListAsync();

            // =====================================================
            // STATISTICS
            // =====================================================

            var allBills = _context.Bills
                .AsNoTracking();

            ViewBag.TotalBills =
                await allBills.CountAsync();

            ViewBag.PaidBills =
                await allBills.CountAsync(b =>
                    b.Status == "Paid");

            ViewBag.UnpaidBills =
                await allBills.CountAsync(b =>
                    b.Status == "Unpaid");

            ViewBag.PartialBills =
                await allBills.CountAsync(b =>
                    b.Status == "Partially Paid" ||
                    b.Status == "Partial");

            ViewBag.OverdueBills =
                await allBills.CountAsync(b =>
                    b.DueDate < DateTime.Today &&
                    b.Status != "Paid");

            ViewBag.OutstandingAmount =
                await allBills
                    .Where(b =>
                        b.Status == "Unpaid" ||
                        b.Status == "Partially Paid" ||
                        b.Status == "Partial")
                    .Select(b => (decimal?)b.TotalAmount)
                    .SumAsync() ?? 0m;

            // =====================================================
            // VIEW DATA
            // =====================================================

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Category = category;

            return View(bills);
        }

        // =========================================================
        // BILL DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var bill = await _context.Bills
                .AsNoTracking()
                .Include(b => b.Customer)
                .Include(b => b.Connection)
                    .ThenInclude(c => c.Plan)
                .FirstOrDefaultAsync(b =>
                    b.BillId == id);

            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }
    }
}