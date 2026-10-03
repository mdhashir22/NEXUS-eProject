using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Retail Employee")]
    public class RetailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RetailController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // RETAIL DASHBOARD
        // =====================================================

        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // =================================================
            // BASIC COUNTS
            // =================================================

            var totalCustomers =
                await _context.Customers.CountAsync();

            var totalOrders =
                await _context.Orders.CountAsync();

            var activeProducts =
                await _context.Products
                    .CountAsync(p =>
                        p.IsActive &&
                        p.Quantity > 0);

            var activeConnections =
                await _context.Connections
                    .CountAsync(c =>
                        c.Status == "Active");

            // =================================================
            // TODAY'S ORDERS
            // =================================================

            var todayOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.CreatedAt >= today &&
                        o.CreatedAt < tomorrow);

            // =================================================
            // PENDING ORDERS
            // =================================================

            var pendingOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.Status == "Pending");

            // =================================================
            // PENDING / UNPAID BILLS
            // =================================================

            var pendingBills =
                await _context.Bills
                    .CountAsync(b =>
                        b.Status == "Unpaid" ||
                        b.Status == "Pending");

            // =================================================
            // TODAY'S PAYMENTS
            // =================================================

            var todayPayments =
                await _context.Payments
                    .CountAsync(p =>
                        p.PaymentDate >= today &&
                        p.PaymentDate < tomorrow &&
                        p.Status == "Paid");

            // =================================================
            // TODAY'S PAYMENT AMOUNT
            // =================================================

            var todayPaymentAmount =
                await _context.Payments
                    .Where(p =>
                        p.PaymentDate >= today &&
                        p.PaymentDate < tomorrow &&
                        p.Status == "Paid")
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            // =================================================
            // OUTSTANDING AMOUNT
            // =================================================

            var outstandingAmount =
                await _context.Bills
                    .Where(b =>
                        b.Status != "Paid")
                    .SumAsync(b =>
                        (decimal?)b.TotalAmount) ?? 0m;

            // =================================================
            // RECENT ORDERS
            // =================================================

            var recentOrders =
                await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Plan)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .ToListAsync();

            // =================================================
            // RECENT PAYMENTS
            // =================================================

            var recentPayments =
                await _context.Payments
                    .Include(p => p.Customer)
                    .Include(p => p.Bill)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .ToListAsync();

            // =================================================
            // VIEW MODEL
            // =================================================

            var dashboard = new RetailDashboardViewModel
            {
                TotalCustomers = totalCustomers,
                TotalOrders = totalOrders,
                ActiveProducts = activeProducts,
                ActiveConnections = activeConnections,

                TodayOrders = todayOrders,
                PendingOrders = pendingOrders,

                PendingBills = pendingBills,
                TodayPayments = todayPayments,

                OutstandingAmount = outstandingAmount,
                TodayPaymentAmount = todayPaymentAmount,

                RecentOrders = recentOrders,
                RecentPayments = recentPayments
            };

            return View(dashboard);
        }
    }
}