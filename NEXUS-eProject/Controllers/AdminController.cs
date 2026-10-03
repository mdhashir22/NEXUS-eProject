using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // ADMIN DASHBOARD
        // =====================================================

        public async Task<IActionResult> Dashboard()
        {
            var now = DateTime.Now;

            var currentMonthStart =
                new DateTime(now.Year, now.Month, 1);

            var nextMonthStart =
                currentMonthStart.AddMonths(1);


            // =================================================
            // LOAD CORE DATA
            // =================================================

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .ToListAsync();

            var connections = await _context.Connections
                .AsNoTracking()
                .Include(c => c.Customer)
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .ToListAsync();

            var bills = await _context.Bills
                .AsNoTracking()
                .ToListAsync();

            var payments = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Customer)
                .Include(p => p.Bill)
                .ToListAsync();


            // =================================================
            // VERIFIED PAYMENTS
            // =================================================

            var verifiedPayments = payments
                .Where(p =>
                    string.Equals(
                        p.Status,
                        "Paid",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();


            // =================================================
            // OUTSTANDING AMOUNT
            // =================================================

            decimal outstandingAmount = 0;

            foreach (var bill in bills)
            {
                var paidAmount = verifiedPayments
                    .Where(p => p.BillId == bill.BillId)
                    .Sum(p => p.Amount);

                var remaining =
                    bill.TotalAmount - paidAmount;

                if (remaining > 0)
                {
                    outstandingAmount += remaining;
                }
            }


            // =================================================
            // DASHBOARD VIEW MODEL
            // =================================================

            var dashboard = new AdminDashboardViewModel
            {
                // ---------------------------------------------
                // SYSTEM
                // ---------------------------------------------

                TotalEmployees =
                    await _context.Employees
                        .AsNoTracking()
                        .CountAsync(),

                TotalShops =
                    await _context.Shops
                        .AsNoTracking()
                        .CountAsync(),

                TotalVendors =
                    await _context.Vendors
                        .AsNoTracking()
                        .CountAsync(),

                TotalProducts =
                    await _context.Products
                        .AsNoTracking()
                        .CountAsync(),

                TotalPlans =
                    await _context.Plans
                        .AsNoTracking()
                        .CountAsync(),

                ActiveEmployees =
                    await _context.Employees
                        .AsNoTracking()
                        .CountAsync(e => e.IsActive),

                ActiveProducts =
                    await _context.Products
                        .AsNoTracking()
                        .CountAsync(p => p.IsActive),

                ActivePlans =
                    await _context.Plans
                        .AsNoTracking()
                        .CountAsync(p => p.IsActive),


                // ---------------------------------------------
                // CUSTOMERS
                // ---------------------------------------------

                TotalCustomers =
                    await _context.Customers
                        .AsNoTracking()
                        .CountAsync(),

                ActiveCustomers =
                    await _context.Customers
                        .AsNoTracking()
                        .CountAsync(c => c.IsActive),


                // ---------------------------------------------
                // ORDERS
                // ---------------------------------------------

                TotalOrders = orders.Count,

                PendingOrders = orders.Count(o =>
                    string.Equals(
                        o.Status,
                        "Pending",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        o.Status,
                        "Submitted",
                        StringComparison.OrdinalIgnoreCase)),

                RetailAcceptedOrders = orders.Count(o =>
                    string.Equals(
                        o.Status,
                        "Retail Accepted",
                        StringComparison.OrdinalIgnoreCase)),

                FeasibleOrders = orders.Count(o =>
                    string.Equals(
                        o.Status,
                        "Feasible",
                        StringComparison.OrdinalIgnoreCase)),

                RejectedOrders = orders.Count(o =>
                    string.Equals(
                        o.Status,
                        "Retail Rejected",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        o.Status,
                        "Technical Rejected",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        o.Status,
                        "Rejected",
                        StringComparison.OrdinalIgnoreCase)),


                // ---------------------------------------------
                // CONNECTIONS
                // ---------------------------------------------

                TotalConnections = connections.Count,

                ActiveConnections = connections.Count(c =>
                    string.Equals(
                        c.Status,
                        "Active",
                        StringComparison.OrdinalIgnoreCase)),

                BillingPendingConnections = connections.Count(c =>
                    string.Equals(
                        c.Status,
                        "Billing Pending",
                        StringComparison.OrdinalIgnoreCase)),

                PaymentPendingConnections = connections.Count(c =>
                    string.Equals(
                        c.Status,
                        "Payment Pending",
                        StringComparison.OrdinalIgnoreCase)),

                PaymentVerifiedConnections = connections.Count(c =>
                    string.Equals(
                        c.Status,
                        "Payment Verified",
                        StringComparison.OrdinalIgnoreCase)),

                InstallationPendingConnections = connections.Count(c =>
                    string.Equals(
                        c.Status,
                        "Installation Pending",
                        StringComparison.OrdinalIgnoreCase)),

                InactiveConnections = connections.Count(c =>
                    string.Equals(
                        c.Status,
                        "Inactive",
                        StringComparison.OrdinalIgnoreCase)),


                // ---------------------------------------------
                // BILLS
                // ---------------------------------------------

                TotalBills = bills.Count,

                PaidBills = bills.Count(b =>
                    string.Equals(
                        b.Status,
                        "Paid",
                        StringComparison.OrdinalIgnoreCase)),

                UnpaidBills = bills.Count(b =>
                    string.Equals(
                        b.Status,
                        "Unpaid",
                        StringComparison.OrdinalIgnoreCase)),

                PartiallyPaidBills = bills.Count(b =>
                    string.Equals(
                        b.Status,
                        "Partially Paid",
                        StringComparison.OrdinalIgnoreCase)),

                TotalBilledAmount =
                    bills.Sum(b => b.TotalAmount),

                OutstandingAmount =
                    outstandingAmount,


                // ---------------------------------------------
                // PAYMENTS / REVENUE
                // ---------------------------------------------

                TotalPayments = payments.Count,

                PendingPayments = payments.Count(p =>
                    string.Equals(
                        p.Status,
                        "Pending Verification",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        p.Status,
                        "Pending",
                        StringComparison.OrdinalIgnoreCase)),

                VerifiedPayments = verifiedPayments.Count,

                RejectedPayments = payments.Count(p =>
                    string.Equals(
                        p.Status,
                        "Rejected",
                        StringComparison.OrdinalIgnoreCase)),

                TotalRevenue =
                    verifiedPayments.Sum(p => p.Amount),

                CurrentMonthRevenue =
                    verifiedPayments
                        .Where(p =>
                            p.PaymentDate >= currentMonthStart &&
                            p.PaymentDate < nextMonthStart)
                        .Sum(p => p.Amount),


                // ---------------------------------------------
                // OPERATIONS
                // ---------------------------------------------

                ConnectionsAwaitingInstallation =
                    connections.Count(c =>
                        string.Equals(
                            c.Status,
                            "Payment Verified",
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(
                            c.Status,
                            "Installation Pending",
                            StringComparison.OrdinalIgnoreCase)),

                ConnectionsActivatedThisMonth =
                    connections.Count(c =>
                        c.ActivatedAt.HasValue &&
                        c.ActivatedAt.Value >= currentMonthStart &&
                        c.ActivatedAt.Value < nextMonthStart),


                // ---------------------------------------------
                // RECENT ACTIVITY
                // ---------------------------------------------

                RecentOrders = orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .ToList(),

                RecentPayments = payments
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .ToList(),

                RecentConnections = connections
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToList()
            };


            // =================================================
            // LAST 6 MONTHS
            // APPLICATIONS + REVENUE
            // =================================================

            for (var i = 5; i >= 0; i--)
            {
                var monthStart =
                    new DateTime(
                        now.Year,
                        now.Month,
                        1)
                    .AddMonths(-i);

                var monthEnd =
                    monthStart.AddMonths(1);


                // ---------------------------------------------
                // LABEL
                // ---------------------------------------------

                var monthLabel =
                    monthStart.ToString("MMM yyyy");

                dashboard.ApplicationChartLabels
                    .Add(monthLabel);

                dashboard.RevenueChartLabels
                    .Add(monthLabel);


                // ---------------------------------------------
                // APPLICATIONS
                // ---------------------------------------------

                var monthlyApplications =
                    orders.Count(o =>
                        o.CreatedAt >= monthStart &&
                        o.CreatedAt < monthEnd);

                dashboard.ApplicationChartData
                    .Add(monthlyApplications);


                // ---------------------------------------------
                // REVENUE
                // ---------------------------------------------

                var monthlyRevenue =
                    verifiedPayments
                        .Where(p =>
                            p.PaymentDate >= monthStart &&
                            p.PaymentDate < monthEnd)
                        .Sum(p => p.Amount);

                dashboard.RevenueChartData
                    .Add(monthlyRevenue);
            }


            // =================================================
            // CONNECTION STATUS CHART
            // =================================================

            var connectionStatusGroups = connections
                .GroupBy(c =>
                    string.IsNullOrWhiteSpace(c.Status)
                        ? "Unknown"
                        : c.Status)
                .OrderByDescending(g => g.Count())
                .ToList();

            foreach (var group in connectionStatusGroups)
            {
                dashboard.ConnectionStatusLabels
                    .Add(group.Key);

                dashboard.ConnectionStatusData
                    .Add(group.Count());
            }


            // =================================================
            // CONNECTION TYPE CHART
            // =================================================

            var connectionTypeGroups = connections
                .GroupBy(c =>
                    string.IsNullOrWhiteSpace(c.ConnectionType)
                        ? "Unknown"
                        : c.ConnectionType)
                .OrderByDescending(g => g.Count())
                .ToList();

            foreach (var group in connectionTypeGroups)
            {
                dashboard.ConnectionTypeLabels
                    .Add(group.Key);

                dashboard.ConnectionTypeData
                    .Add(group.Count());
            }


            // =================================================
            // BILL STATUS CHART
            // =================================================

            var billStatusGroups = bills
                .GroupBy(b =>
                    string.IsNullOrWhiteSpace(b.Status)
                        ? "Unknown"
                        : b.Status)
                .OrderByDescending(g => g.Count())
                .ToList();

            foreach (var group in billStatusGroups)
            {
                dashboard.BillStatusLabels
                    .Add(group.Key);

                dashboard.BillStatusData
                    .Add(group.Count());
            }


            return View(dashboard);
        }
    }
}