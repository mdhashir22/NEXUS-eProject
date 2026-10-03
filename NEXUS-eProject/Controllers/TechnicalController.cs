using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Technical Employee")]
    public class TechnicalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TechnicalController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var model = new TechnicalDashboardViewModel
            {
                PendingOrders =
                    await _context.Orders.CountAsync(o =>
                        o.Status == "Retail Accepted"),

                FeasibilityPending =
                    await _context.Orders.CountAsync(o =>
                        o.Status == "Retail Accepted"),

                FeasibleOrders =
                    await _context.Orders.CountAsync(o =>
                        o.Status == "Feasible"),

                RejectedOrders =
                    await _context.Orders.CountAsync(o =>
                        o.Status == "Technical Rejected"),

                TotalConnections =
                    await _context.Connections.CountAsync(),

                ActiveConnections =
                    await _context.Connections.CountAsync(c =>
                        c.Status == "Active"),

                PaymentVerified =
                    await _context.Orders.CountAsync(o =>
                        o.Status == "Payment Verified"),

                InstallationPending =
                    await _context.Orders.CountAsync(o =>
                        o.Status == "Installation Pending"),

                AvailableEquipment =
                    await _context.Products.CountAsync(p =>
                        p.IsActive &&
                        p.Quantity > 0),

                PendingTechnicalOrders =
                    await _context.Orders
                        .AsNoTracking()
                        .Include(o => o.Customer)
                        .Include(o => o.Plan)
                        .Where(o =>
                            o.Status == "Retail Accepted")
                        .OrderByDescending(o => o.CreatedAt)
                        .Take(5)
                        .ToListAsync(),

                RecentTechnicalOrders =
                    await _context.Orders
                        .AsNoTracking()
                        .Include(o => o.Customer)
                        .Include(o => o.Plan)
                        .Where(o =>
                            o.Status == "Retail Accepted" ||
                            o.Status == "Feasible" ||
                            o.Status == "Technical Rejected" ||
                            o.Status == "Connection Created" ||
                            o.Status == "Billing Pending" ||
                            o.Status == "Payment Pending" ||
                            o.Status == "Payment Verified" ||
                            o.Status == "Installation Pending" ||
                            o.Status == "Active")
                        .OrderByDescending(o =>
                            o.UpdatedAt ?? o.CreatedAt)
                        .Take(5)
                        .ToListAsync(),

                RecentConnections =
                    await _context.Connections
                        .AsNoTracking()
                        .Include(c => c.Customer)
                        .Include(c => c.Plan)
                        .Include(c => c.Order)
                        .OrderByDescending(c => c.CreatedAt)
                        .Take(5)
                        .ToListAsync()
            };

            return View(model);
        }
    }
}