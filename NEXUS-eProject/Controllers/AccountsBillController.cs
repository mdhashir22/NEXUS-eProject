using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Accounts Employee")]
    public class AccountsBillController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsBillController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // ALL BILLS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status)
        {
            var query = _context.Bills
                .AsNoTracking()
                .Include(b => b.Customer)
                .Include(b => b.Connection)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(b =>
                    b.BillNumber.Contains(search) ||
                    b.AccountId.Contains(search) ||
                    (b.Customer != null &&
                     b.Customer.FullName.Contains(search)));
            }


            if (!string.IsNullOrWhiteSpace(status) &&
                status != "All")
            {
                query = query.Where(b =>
                    b.Status == status);
            }


            var bills = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();


            ViewBag.Search = search;
            ViewBag.Status = status;


            return View(bills);
        }


        // =====================================================
        // BILLING PENDING
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Pending(string? search)
        {
            var query = _context.Connections
                .AsNoTracking()
                .Include(c => c.Customer)
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .Where(c =>
                    c.Order != null &&
                    (
                        c.Order.Status == "Connection Created" ||
                        c.Order.Status == "Billing Pending"
                    ))
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>

                    c.AccountId.Contains(search) ||

                    (c.Customer != null &&
                     c.Customer.FullName.Contains(search)) ||

                    (c.Customer != null &&
                     c.Customer.Phone != null &&
                     c.Customer.Phone.Contains(search)) ||

                    (c.Order != null &&
                     c.Order.OrderNumber.Contains(search))
                );
            }


            var connections = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();


            ViewBag.Search = search;


            return View(connections);
        }


        // =====================================================
        // CREATE BILL - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create(int connectionId)
        {
            var connection =
                await GetConnectionAsync(connectionId);


            if (connection == null)
            {
                TempData["Error"] =
                    "Connection could not be found.";

                return RedirectToAction(nameof(Pending));
            }


            if (connection.Order == null ||
                (
                    connection.Order.Status != "Connection Created" &&
                    connection.Order.Status != "Billing Pending"
                ))
            {
                TempData["Error"] =
                    "This connection is not available for billing.";

                return RedirectToAction(nameof(Pending));
            }


            // Prevent duplicate bill
            var existingBill =
                await _context.Bills
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b =>
                        b.ConnectionId ==
                        connection.ConnectionId);


            if (existingBill != null)
            {
                TempData["Error"] =
                    "A bill already exists for this connection.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = existingBill.BillId });
            }


            // =================================================
            // LOAD ADMIN SYSTEM SETTINGS
            // =================================================

            var settings =
                await GetSystemSettingsAsync();


            // Previous unpaid amount for this account
            var previousDue =
                await _context.Bills
                    .Where(b =>
                        b.AccountId == connection.AccountId &&
                        (
                            b.Status == "Unpaid" ||
                            b.Status == "Partially Paid"
                        ))
                    .SumAsync(b =>
                        (decimal?)b.TotalAmount) ?? 0m;


            // Try to find equipment price automatically
            decimal equipmentAmount = 0m;


            if (!string.IsNullOrWhiteSpace(connection.Equipment))
            {
                var equipmentName =
                    connection.Equipment.Trim();


                var product =
                    await _context.Products
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p =>
                            p.ProductName == equipmentName);


                if (product != null)
                {
                    equipmentAmount =
                        product.Price;
                }
            }


            var model =
                new AccountsBillCreateViewModel
                {
                    ConnectionId =
                        connection.ConnectionId,

                    AccountId =
                        connection.AccountId ?? string.Empty,

                    OrderNumber =
                        connection.Order.OrderNumber,

                    CustomerName =
                        connection.Customer?.FullName
                        ?? string.Empty,

                    Phone =
                        connection.Customer?.Phone
                        ?? string.Empty,

                    ConnectionType =
                        connection.ConnectionType
                        ?? string.Empty,

                    PlanName =
                        connection.Plan?.PlanName
                        ?? string.Empty,

                    Speed =
                        connection.Plan?.Speed
                        ?? string.Empty,

                    Equipment =
                        connection.Equipment
                        ?? string.Empty,

                    PlanAmount =
                        connection.Plan?.Price ?? 0m,

                    EquipmentAmount =
                        equipmentAmount,

                    PreviousDue =
                        previousDue,

                    Discount =
                        0m,

                    // Comes from Admin Settings
                    TaxPercentage =
                        settings.TaxPercentage,

                    BillingDate =
                        DateTime.Today,

                    // Comes from Admin Settings
                    DueDate =
                        DateTime.Today.AddDays(
                            settings.DefaultBillDueDays)
                };


            ViewBag.Currency =
                settings.Currency;


            return View(model);
        }


        // =====================================================
        // CREATE BILL - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AccountsBillCreateViewModel model)
        {
            // =================================================
            // LOAD CONNECTION FROM DATABASE
            // =================================================

            var connection =
                await GetConnectionAsync(model.ConnectionId);


            if (connection == null)
            {
                TempData["Error"] =
                    "Connection could not be found.";

                return RedirectToAction(nameof(Pending));
            }


            // =================================================
            // LOAD ADMIN SYSTEM SETTINGS
            // =================================================

            var settings =
                await GetSystemSettingsAsync();


            /*
             * IMPORTANT:
             * Tax percentage is controlled by Admin Settings.
             * Never trust a tax percentage posted by the browser.
             */
            model.TaxPercentage =
                settings.TaxPercentage;


            ViewBag.Currency =
                settings.Currency;


            // Re-populate display-only information.
            PopulateDisplayData(
                model,
                connection);


            // =================================================
            // CHECK WORKFLOW STATUS
            // =================================================

            if (connection.Order == null ||
                (
                    connection.Order.Status != "Connection Created" &&
                    connection.Order.Status != "Billing Pending"
                ))
            {
                TempData["Error"] =
                    "This connection is no longer available for billing.";

                return RedirectToAction(nameof(Pending));
            }


            // =================================================
            // DUPLICATE BILL CHECK
            // =================================================

            var existingBill =
                await _context.Bills
                    .FirstOrDefaultAsync(b =>
                        b.ConnectionId ==
                        connection.ConnectionId);


            if (existingBill != null)
            {
                TempData["Error"] =
                    "A bill has already been generated for this connection.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = existingBill.BillId });
            }


            // =================================================
            // CUSTOM VALIDATION
            // =================================================

            if (model.BillingDate == default)
            {
                ModelState.AddModelError(
                    nameof(model.BillingDate),
                    "Billing date is required.");
            }


            if (model.DueDate == default)
            {
                /*
                 * If the date was not posted for any reason,
                 * use the configured billing period.
                 */
                model.DueDate =
                    model.BillingDate.Date.AddDays(
                        settings.DefaultBillDueDays);
            }


            if (model.DueDate.Date <
                model.BillingDate.Date)
            {
                ModelState.AddModelError(
                    nameof(model.DueDate),
                    "Due date cannot be before billing date.");
            }


            // Plan price must come from database
            model.PlanAmount =
                connection.Plan?.Price ?? 0m;


            // Previous due is recalculated server-side
            model.PreviousDue =
                await _context.Bills
                    .Where(b =>
                        b.AccountId == connection.AccountId &&
                        (
                            b.Status == "Unpaid" ||
                            b.Status == "Partially Paid"
                        ))
                    .SumAsync(b =>
                        (decimal?)b.TotalAmount) ?? 0m;


            if (model.EquipmentAmount < 0)
            {
                ModelState.AddModelError(
                    nameof(model.EquipmentAmount),
                    "Equipment amount cannot be negative.");
            }


            if (model.Discount < 0)
            {
                ModelState.AddModelError(
                    nameof(model.Discount),
                    "Discount cannot be negative.");
            }


            var grossAmount =
                model.PlanAmount +
                model.EquipmentAmount +
                model.PreviousDue;


            if (model.Discount > grossAmount)
            {
                ModelState.AddModelError(
                    nameof(model.Discount),
                    "Discount cannot be greater than the bill amount.");
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =================================================
            // BILL CALCULATION
            // =================================================

            var amountAfterDiscount =
                grossAmount -
                model.Discount;


            if (amountAfterDiscount < 0)
            {
                amountAfterDiscount = 0;
            }


            /*
             * Tax percentage is now always the value configured
             * by the Administrator in System Settings.
             */
            var tax =
                Math.Round(
                    amountAfterDiscount *
                    (settings.TaxPercentage / 100m),
                    2);


            var totalAmount =
                Math.Round(
                    amountAfterDiscount + tax,
                    2);


            // =================================================
            // GENERATE BILL NUMBER
            // =================================================

            var lastBillId =
                await _context.Bills
                    .MaxAsync(b =>
                        (int?)b.BillId) ?? 0;


            var nextBillId =
                lastBillId + 1;


            var billNumber =
                $"INV{nextBillId:D10}";


            // =================================================
            // CREATE BILL
            // =================================================

            var bill =
                new Bill
                {
                    BillNumber =
                        billNumber,

                    CustomerId =
                        connection.CustomerId,

                    ConnectionId =
                        connection.ConnectionId,

                    AccountId =
                        connection.AccountId,

                    PlanAmount =
                        model.PlanAmount,

                    EquipmentAmount =
                        model.EquipmentAmount,

                    PreviousDue =
                        model.PreviousDue,

                    Discount =
                        model.Discount,

                    Tax =
                        tax,

                    TotalAmount =
                        totalAmount,

                    BillingDate =
                        model.BillingDate.Date,

                    DueDate =
                        model.DueDate.Date,

                    Status =
                        "Unpaid",

                    CreatedAt =
                        DateTime.Now
                };


            _context.Bills.Add(bill);


            // =================================================
            // UPDATE WORKFLOW
            // =================================================

            connection.Order.Status =
                "Payment Pending";


            connection.Order.UpdatedAt =
                DateTime.Now;


            /*
             * Connection is created but not active yet.
             * Accounts has completed billing and payment
             * is now required from the customer.
             */
            connection.Status =
                "Billing Pending";


            // =================================================
            // SAVE EVERYTHING
            // =================================================

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Bill could not be saved. Please check the billing information and try again.");

                return View(model);
            }


            // =================================================
            // SUCCESS
            // =================================================

            TempData["Success"] =
                $"Bill {bill.BillNumber} generated successfully.";


            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = bill.BillId
                });
        }


        // =====================================================
        // BILL DETAILS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var bill =
                await _context.Bills
                    .AsNoTracking()

                    .Include(b =>
                        b.Customer)

                    .Include(b =>
                        b.Connection)
                        .ThenInclude(c =>
                            c.Plan)

                    .Include(b =>
                        b.Connection)
                        .ThenInclude(c =>
                            c.Order)

                    .FirstOrDefaultAsync(b =>
                        b.BillId == id);


            if (bill == null)
            {
                return NotFound();
            }


            // Currency is available to the details view
            var settings =
                await GetSystemSettingsAsync();


            ViewBag.Currency =
                settings.Currency;


            return View(bill);
        }


        // =====================================================
        // PRIVATE - GET CONNECTION
        // =====================================================

        private async Task<Connection?>
            GetConnectionAsync(int connectionId)
        {
            return await _context.Connections

                .Include(c =>
                    c.Customer)

                .Include(c =>
                    c.Plan)

                .Include(c =>
                    c.Order)

                .FirstOrDefaultAsync(c =>
                    c.ConnectionId ==
                    connectionId);
        }


        // =====================================================
        // PRIVATE - GET SYSTEM SETTINGS
        // =====================================================

        private async Task<SystemSetting>
            GetSystemSettingsAsync()
        {
            var settings =
                await _context.SystemSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync();


            /*
             * Safe fallback:
             * Billing must continue working even if the settings
             * row is accidentally missing from the database.
             */
            if (settings == null)
            {
                return new SystemSetting
                {
                    ApplicationName = "NEXUS",

                    TaxPercentage = 12.24m,

                    DefaultBillDueDays = 10,

                    Currency = "PKR",

                    CityCode = "101",

                    BroadbandPrefix = "B",

                    TelephonePrefix = "T",

                    DialUpPrefix = "D",

                    CustomerRegistrationEnabled = true,

                    FeedbackEnabled = true,

                    MaintenanceMode = false
                };
            }


            return settings;
        }


        // =====================================================
        // PRIVATE - POPULATE DISPLAY DATA
        // =====================================================

        private void PopulateDisplayData(
            AccountsBillCreateViewModel model,
            Connection connection)
        {
            model.AccountId =
                connection.AccountId
                ?? string.Empty;


            model.OrderNumber =
                connection.Order?.OrderNumber
                ?? string.Empty;


            model.CustomerName =
                connection.Customer?.FullName
                ?? string.Empty;


            model.Phone =
                connection.Customer?.Phone
                ?? string.Empty;


            model.ConnectionType =
                connection.ConnectionType
                ?? string.Empty;


            model.PlanName =
                connection.Plan?.PlanName
                ?? string.Empty;


            model.Speed =
                connection.Plan?.Speed
                ?? string.Empty;


            model.Equipment =
                connection.Equipment
                ?? string.Empty;
        }
    }
}