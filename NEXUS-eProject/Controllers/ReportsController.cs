using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalEmployees = await _context.Employees.CountAsync();
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalPlans = await _context.Plans.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalConnections = await _context.Connections.CountAsync();

            ViewBag.PendingOrders = await _context.Orders
                .CountAsync(o => o.Status == "Pending");

            ViewBag.ApprovedOrders = await _context.Orders
                .CountAsync(o => o.Status == "Approved");

            ViewBag.ActiveConnections = await _context.Connections
                .CountAsync(c => c.Status == "Active");

            ViewBag.PendingConnections = await _context.Connections
                .CountAsync(c => c.Status != "Active");

            return View();
        }
    }
}