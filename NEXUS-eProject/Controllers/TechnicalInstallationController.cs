using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Technical Employee")]
    public class TechnicalInstallationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TechnicalInstallationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Connections
                .AsNoTracking()
                .Include(c => c.Customer)
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .Where(c =>
                    c.Order != null &&
                    (c.Order.Status == "Payment Verified" ||
                     c.Order.Status == "Installation Pending"))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.AccountId.Contains(search) ||
                    (c.Order != null &&
                     c.Order.OrderNumber.Contains(search)) ||
                    (c.Customer != null &&
                     c.Customer.FullName.Contains(search)) ||
                    (c.Customer != null &&
                     c.Customer.Phone != null &&
                     c.Customer.Phone.Contains(search)));
            }

            var connections = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;

            return View(connections);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var connection = await _context.Connections
                .AsNoTracking()
                .Include(c => c.Customer)
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.ConnectionId == id);

            if (connection == null)
                return NotFound();

            if (connection.Order == null ||
                (connection.Order.Status != "Payment Verified" &&
                 connection.Order.Status != "Installation Pending"))
            {
                TempData["Error"] =
                    "This connection is not ready for installation.";

                return RedirectToAction(nameof(Index));
            }

            var model = new TechnicalInstallationViewModel
            {
                ConnectionId = connection.ConnectionId,
                OrderId = connection.OrderId,
                AccountId = connection.AccountId,
                OrderNumber = connection.Order.OrderNumber ?? string.Empty,

                CustomerName =
                    connection.Customer?.FullName ?? string.Empty,

                Phone =
                    connection.Customer?.Phone ?? string.Empty,

                Address =
                    connection.Customer?.Address ?? string.Empty,

                City =
                    connection.Customer?.City ?? string.Empty,

                ConnectionType =
                    connection.ConnectionType ?? string.Empty,

                PlanName =
                    connection.Plan?.PlanName ?? string.Empty,

                Speed =
                    connection.Plan?.Speed ?? string.Empty,

                Equipment =
                    connection.Equipment ?? string.Empty,

                RouterSerial =
                    connection.RouterSerial ?? string.Empty,

                ConnectionStatus =
                    connection.Status ?? "Pending",

                OrderStatus =
                    connection.Order.Status,

                InstallationDate =
                    connection.InstallationDate,

                ActivatedAt =
                    connection.ActivatedAt
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartInstallation(int id)
        {
            var connection = await _context.Connections
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.ConnectionId == id);

            if (connection == null)
                return NotFound();

            if (connection.Order == null ||
                connection.Order.Status != "Payment Verified")
            {
                TempData["Error"] =
                    "Payment must be verified before installation.";

                return RedirectToAction(nameof(Index));
            }

            connection.Status = "Installation Pending";
            connection.Order.Status = "Installation Pending";
            connection.Order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Installation process has been started.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteInstallation(
            int id,
            string? routerSerial)
        {
            var connection = await _context.Connections
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.ConnectionId == id);

            if (connection == null)
                return NotFound();

            if (connection.Order == null ||
                connection.Order.Status != "Installation Pending")
            {
                TempData["Error"] =
                    "Installation has not been started.";

                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrWhiteSpace(routerSerial))
            {
                connection.RouterSerial = routerSerial.Trim();
            }

            connection.InstallationDate = DateTime.Now;
            connection.ActivatedAt = DateTime.Now;
            connection.Status = "Active";

            connection.Order.Status = "Active";
            connection.Order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Connection {connection.AccountId} installed and activated successfully.";

            return RedirectToAction(
                "Details",
                "TechnicalConnection",
                new { id = connection.ConnectionId });
        }
    }
}