using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Technical Employee")]
    public class TechnicalOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TechnicalOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // ALL TECHNICAL ORDERS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? connectionType)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .AsQueryable();

            query = query.Where(o =>
                o.Status == "Retail Accepted" ||
                o.Status == "Feasible" ||
                o.Status == "Technical Rejected" ||
                o.Status == "Connection Created" ||
                o.Status == "Billing Pending" ||
                o.Status == "Payment Pending" ||
                o.Status == "Payment Verified" ||
                o.Status == "Installation Pending" ||
                o.Status == "Active");

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(o =>
                    o.OrderNumber.Contains(search) ||
                    (o.Customer != null &&
                     o.Customer.FullName.Contains(search)) ||
                    (o.Customer != null &&
                     o.Customer.Phone != null &&
                     o.Customer.Phone.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(connectionType))
            {
                query = query.Where(o =>
                    o.ConnectionType == connectionType);
            }

            var orders = await query
                .OrderByDescending(o => o.UpdatedAt ?? o.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.ConnectionType = connectionType;

            return View(orders);
        }

        // =========================================================
        // PENDING TECHNICAL ORDERS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Pending(
            string? search,
            string? connectionType)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .Where(o => o.Status == "Retail Accepted")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(o =>
                    o.OrderNumber.Contains(search) ||
                    (o.Customer != null &&
                     o.Customer.FullName.Contains(search)) ||
                    (o.Customer != null &&
                     o.Customer.Phone != null &&
                     o.Customer.Phone.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(connectionType))
            {
                query = query.Where(o =>
                    o.ConnectionType == connectionType);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.ConnectionType = connectionType;
            ViewBag.PageTitle = "Pending Technical Orders";
            ViewBag.PageDescription =
                "Orders accepted by Retail and waiting for technical processing.";

            return View(orders);
        }

        // =========================================================
        // FEASIBILITY QUEUE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Feasibility(
            string? search,
            string? connectionType)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .Where(o => o.Status == "Retail Accepted")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(o =>
                    o.OrderNumber.Contains(search) ||
                    (o.Customer != null &&
                     o.Customer.FullName.Contains(search)) ||
                    (o.Customer != null &&
                     o.Customer.Phone != null &&
                     o.Customer.Phone.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(connectionType))
            {
                query = query.Where(o =>
                    o.ConnectionType == connectionType);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.ConnectionType = connectionType;
            ViewBag.PageTitle = "Feasibility Checks";
            ViewBag.PageDescription =
                "Review applications that require technical feasibility checking.";

            return View(orders);
        }

        // =========================================================
        // ORDER DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .FirstOrDefaultAsync(o =>
                    o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var validOrder =
                order.Status == "Retail Accepted" ||
                order.Status == "Feasible" ||
                order.Status == "Technical Rejected" ||
                order.Status == "Connection Created" ||
                order.Status == "Billing Pending" ||
                order.Status == "Payment Pending" ||
                order.Status == "Payment Verified" ||
                order.Status == "Installation Pending" ||
                order.Status == "Active";

            if (!validOrder)
            {
                return NotFound();
            }

            return View(order);
        }

        // =========================================================
        // FEASIBILITY CHECK
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> CheckFeasibility(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .FirstOrDefaultAsync(o =>
                    o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Retail Accepted")
            {
                TempData["Error"] =
                    "This order is not waiting for technical feasibility.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            return View(order);
        }

        // =========================================================
        // MARK FEASIBLE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFeasible(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Retail Accepted")
            {
                TempData["Error"] =
                    "This order has already been processed.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            order.Status = "Feasible";
            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Order {order.OrderNumber} has been marked as technically feasible.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // =========================================================
        // REJECT TECHNICALLY
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectTechnical(
            int id,
            string? reason)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Retail Accepted")
            {
                TempData["Error"] =
                    "This order has already been processed.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] =
                    "Please provide a technical rejection reason.";

                return RedirectToAction(
                    nameof(CheckFeasibility),
                    new { id });
            }

            order.Status = "Technical Rejected";

            order.AdditionalInfo =
                string.IsNullOrWhiteSpace(order.AdditionalInfo)
                    ? $"Technical Rejection Reason: {reason.Trim()}"
                    : $"{order.AdditionalInfo}\n\nTechnical Rejection Reason: {reason.Trim()}";

            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Order {order.OrderNumber} has been technically rejected.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }
}