using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Retail Employee")]
    public class RetailOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RetailOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

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

            // Actual orders = applications accepted by Retail
            query = query.Where(o =>
                o.Status == "Retail Accepted" ||
                o.Status == "Feasible" ||
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

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // Retail Orders page should only show accepted/processed orders
            var validOrder = order.Status == "Retail Accepted" ||
                             order.Status == "Feasible" ||
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
    }
}