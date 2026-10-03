using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Technical Employee")]
    public class TechnicalConnectionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TechnicalConnectionController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // CONNECTION LIST
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? connectionType)
        {
            var query = _context.Connections
                .AsNoTracking()
                .Include(c => c.Customer)
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.AccountId.Contains(search) ||
                    c.ConnectionType.Contains(search) ||
                    (c.Customer != null &&
                     c.Customer.FullName.Contains(search)) ||
                    (c.Customer != null &&
                     c.Customer.Phone != null &&
                     c.Customer.Phone.Contains(search)));
            }


            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c =>
                    c.Status == status);
            }


            if (!string.IsNullOrWhiteSpace(connectionType))
            {
                query = query.Where(c =>
                    c.ConnectionType == connectionType);
            }


            var connections = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();


            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.ConnectionType = connectionType;


            return View(connections);
        }


        // =========================================================
        // CONNECTION DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var connection = await _context.Connections
                .AsNoTracking()
                .Include(c => c.Customer)
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c =>
                    c.ConnectionId == id);


            if (connection == null)
            {
                return NotFound();
            }


            return View(connection);
        }


        // =========================================================
        // CREATE CONNECTION PAGE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create(int orderId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .FirstOrDefaultAsync(o =>
                    o.OrderId == orderId);


            if (order == null)
            {
                return NotFound();
            }


            if (order.Status != "Feasible")
            {
                TempData["Error"] =
                    "Only technically feasible orders can create a connection.";

                return RedirectToAction(
                    "Details",
                    "TechnicalOrder",
                    new { id = orderId });
            }


            var existingConnection =
                await _context.Connections
                    .AsNoTracking()
                    .AnyAsync(c =>
                        c.OrderId == orderId);


            if (existingConnection)
            {
                TempData["Error"] =
                    "A connection has already been created for this order.";

                return RedirectToAction(
                    "Details",
                    "TechnicalOrder",
                    new { id = orderId });
            }


            return View(order);
        }


        // =========================================================
        // CREATE CONNECTION
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int orderId,
            string? routerSerial,
            DateTime? installationDate)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Plan)
                .FirstOrDefaultAsync(o =>
                    o.OrderId == orderId);


            if (order == null)
            {
                return NotFound();
            }


            if (order.Status != "Feasible")
            {
                TempData["Error"] =
                    "Only technically feasible orders can create a connection.";

                return RedirectToAction(
                    "Details",
                    "TechnicalOrder",
                    new { id = orderId });
            }


            var existingConnection =
                await _context.Connections
                    .FirstOrDefaultAsync(c =>
                        c.OrderId == orderId);


            if (existingConnection != null)
            {
                TempData["Error"] =
                    "A connection already exists for this order.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = existingConnection.ConnectionId
                    });
            }


            if (string.IsNullOrWhiteSpace(routerSerial))
            {
                TempData["Error"] =
                    "Please enter the router serial number.";

                return RedirectToAction(
                    nameof(Create),
                    new { orderId });
            }


            routerSerial = routerSerial.Trim();


            // =====================================================
            // GENERATE ACCOUNT ID
            // =====================================================

            var accountId =
                await GenerateAccountIdAsync(
                    order.ConnectionType);


            // =====================================================
            // CREATE CONNECTION
            // =====================================================

            var connection = new Connection
            {
                AccountId = accountId,

                OrderId = order.OrderId,

                CustomerId = order.CustomerId,

                ConnectionType = order.ConnectionType,

                PlanId = order.PlanId,

                Equipment = order.Equipment,

                RouterSerial = routerSerial,

                InstallationDate =
                    installationDate ?? DateTime.Now,

                Status = "Pending",

                CreatedAt = DateTime.Now
            };


            _context.Connections.Add(connection);


            // =====================================================
            // UPDATE ORDER STATUS
            // =====================================================

            order.Status =
                "Connection Created";

            order.UpdatedAt =
                DateTime.Now;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"Connection created successfully. Account ID: {accountId}";


            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = connection.ConnectionId
                });
        }


        // =========================================================
        // ACTIVE CONNECTIONS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Active(
            string? search,
            string? connectionType)
        {
            var query = _context.Connections
                .AsNoTracking()
                .Include(c => c.Customer)
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .Where(c =>
                    c.Status == "Active")
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
                     c.Customer.Phone.Contains(search)));
            }


            if (!string.IsNullOrWhiteSpace(connectionType))
            {
                query = query.Where(c =>
                    c.ConnectionType == connectionType);
            }


            var connections = await query
                .OrderByDescending(c =>
                    c.ActivatedAt ?? c.CreatedAt)
                .ToListAsync();


            ViewBag.Search = search;
            ViewBag.ConnectionType = connectionType;

            ViewBag.PageTitle =
                "Active Connections";

            ViewBag.PageDescription =
                "Currently active NEXUS customer connections.";


            return View(connections);
        }


        // =========================================================
        // ACCOUNT ID GENERATOR
        // =========================================================

        private async Task<string> GenerateAccountIdAsync(
            string connectionType)
        {
            // =====================================================
            // LOAD ADMIN SYSTEM SETTINGS
            // =====================================================

            var settings =
                await GetSystemSettingsAsync();


            // =====================================================
            // CITY CODE
            // =====================================================

            var cityCode =
                string.IsNullOrWhiteSpace(settings.CityCode)
                    ? "101"
                    : settings.CityCode.Trim();


            // =====================================================
            // CONNECTION PREFIX
            // =====================================================

            string prefix;


            switch (
                connectionType
                    .Trim()
                    .ToLowerInvariant())
            {
                case "broadband":
                case "broadband internet":
                case "b":

                    prefix =
                        string.IsNullOrWhiteSpace(
                            settings.BroadbandPrefix)
                            ? "B"
                            : settings.BroadbandPrefix
                                .Trim()
                                .ToUpperInvariant();

                    break;


                case "dial-up":
                case "dialup":
                case "dial-up internet":
                case "d":

                    prefix =
                        string.IsNullOrWhiteSpace(
                            settings.DialUpPrefix)
                            ? "D"
                            : settings.DialUpPrefix
                                .Trim()
                                .ToUpperInvariant();

                    break;


                case "telephone":
                case "telephone/landline":
                case "landline":
                case "t":

                    prefix =
                        string.IsNullOrWhiteSpace(
                            settings.TelephonePrefix)
                            ? "T"
                            : settings.TelephonePrefix
                                .Trim()
                                .ToUpperInvariant();

                    break;


                default:

                    prefix =
                        connectionType
                            .Trim()
                            .Substring(0, 1)
                            .ToUpperInvariant();

                    break;
            }


            // =====================================================
            // ACCOUNT ID BASE
            // =====================================================

            var accountBase =
                prefix + cityCode;


            // =====================================================
            // FIND EXISTING ACCOUNTS FOR THIS PREFIX + CITY
            // =====================================================

            var existingAccounts =
                await _context.Connections
                    .AsNoTracking()
                    .Where(c =>
                        c.AccountId != null &&
                        c.AccountId.StartsWith(accountBase))
                    .Select(c =>
                        c.AccountId)
                    .ToListAsync();


            // =====================================================
            // GENERATE NEXT SERIAL
            // =====================================================

            long nextSerial = 1;


            foreach (var accountId in existingAccounts)
            {
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    continue;
                }


                /*
                 * Do NOT use a fixed Substring(4).
                 *
                 * Prefix and City Code are now configurable,
                 * therefore their combined length can change.
                 */
                if (accountId.Length <= accountBase.Length)
                {
                    continue;
                }


                var serialPart =
                    accountId.Substring(
                        accountBase.Length);


                if (long.TryParse(
                    serialPart,
                    out long serial))
                {
                    if (serial >= nextSerial)
                    {
                        nextSerial =
                            serial + 1;
                    }
                }
            }


            // =====================================================
            // FINAL ACCOUNT ID
            // =====================================================

            return
                $"{accountBase}{nextSerial:D12}";
        }


        // =========================================================
        // GET SYSTEM SETTINGS
        // =========================================================

        private async Task<SystemSetting>
            GetSystemSettingsAsync()
        {
            var settings =
                await _context.SystemSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync();


            /*
             * Safe fallback.
             *
             * Connection creation must continue working even
             * if the settings row is accidentally unavailable.
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
    }
}