using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Retail Employee")]
    public class RetailOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RetailOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        private static bool IsRetailPending(string? status)
        {
            return string.IsNullOrWhiteSpace(status)
                || status == "Pending"
                || status == "Submitted";
        }

        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? connectionType)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Plan)
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
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.ConnectionType = connectionType;

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> Review(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var model = new RetailApplicationViewModel
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,

                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName,
                FatherName = order.Customer?.FatherName,
                Phone = order.Customer?.Phone,
                Email = order.Customer?.Email,
                Address = order.Customer?.Address,
                City = order.Customer?.City,

                ConnectionType = order.ConnectionType,

                PlanId = order.PlanId,
                PlanName = order.Plan?.PlanName,
                PlanSpeed = order.Plan?.Speed,
                PlanPrice = order.Plan?.Price ?? 0m,

                Equipment = order.Equipment,
                AdditionalInfo = order.AdditionalInfo,

                Status = order.Status,

                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            if (!IsRetailPending(order.Status))
            {
                TempData["Error"] =
                    "This application has already been processed.";

                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = "Retail Accepted";
            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Application {order.OrderNumber} has been accepted and forwarded to Technical.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reason)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            if (!IsRetailPending(order.Status))
            {
                TempData["Error"] =
                    "This application has already been processed.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] =
                    "Please provide a rejection reason.";

                return RedirectToAction(nameof(Review), new { id });
            }

            order.Status = "Retail Rejected";

            order.AdditionalInfo =
                string.IsNullOrWhiteSpace(order.AdditionalInfo)
                    ? $"Retail Rejection Reason: {reason.Trim()}"
                    : $"{order.AdditionalInfo}\n\nRetail Rejection Reason: {reason.Trim()}";

            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Application {order.OrderNumber} has been rejected.";

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Status(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> Pending()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .Where(o =>
                    string.IsNullOrWhiteSpace(o.Status) ||
                    o.Status == "Pending" ||
                    o.Status == "Submitted")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.PageTitle = "Pending Applications";
            ViewBag.PageDescription =
                "Applications waiting for retail review.";

            return View(orders);
        }

        public async Task<IActionResult> AcceptedOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .Where(o =>
                    o.Status == "Retail Accepted" ||
                    o.Status == "Feasible" ||
                    o.Status == "Connection Created" ||
                    o.Status == "Billing Pending" ||
                    o.Status == "Payment Pending" ||
                    o.Status == "Payment Verified" ||
                    o.Status == "Installation Pending" ||
                    o.Status == "Active")
                .OrderByDescending(o => o.UpdatedAt ?? o.CreatedAt)
                .ToListAsync();

            ViewBag.PageTitle = "Accepted Applications";
            ViewBag.PageDescription =
                "Applications accepted by retail and moving through the service workflow.";

            return View("Accepted", orders);
        }

        public async Task<IActionResult> Rejected()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .Where(o => o.Status == "Retail Rejected")
                .OrderByDescending(o => o.UpdatedAt ?? o.CreatedAt)
                .ToListAsync();

            ViewBag.PageTitle = "Rejected Applications";
            ViewBag.PageDescription =
                "Applications rejected during retail review.";

            return View(orders);
        }
    }
}