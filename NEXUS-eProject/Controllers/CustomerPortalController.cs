using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerPortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CustomerPortalController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================================
        // CUSTOMER PORTAL HOME
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customer =
                await GetLoggedInCustomerAsync();

            if (customer == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // -----------------------------------------------------
            // TOTAL APPLICATIONS
            // -----------------------------------------------------

            var totalApplications =
                await _context.Orders
                    .CountAsync(o =>
                        o.CustomerId ==
                        customer.CustomerId);


            // -----------------------------------------------------
            // LATEST CONNECTION
            // -----------------------------------------------------

            var connection =
                await _context.Connections
                    .AsNoTracking()
                    .Include(c => c.Plan)
                    .Include(c => c.Order)
                    .Where(c =>
                        c.CustomerId ==
                        customer.CustomerId)
                    .OrderByDescending(c =>
                        c.ConnectionId)
                    .FirstOrDefaultAsync();


            // -----------------------------------------------------
            // CUSTOMER BILLS
            // -----------------------------------------------------

            var bills =
                await _context.Bills
                    .AsNoTracking()
                    .Where(b =>
                        b.CustomerId ==
                        customer.CustomerId)
                    .OrderByDescending(b =>
                        b.BillId)
                    .ToListAsync();


            // -----------------------------------------------------
            // OUTSTANDING AMOUNT
            // Bill total minus VERIFIED payments only
            // -----------------------------------------------------

            decimal outstandingAmount = 0m;

            foreach (var bill in bills.Where(b =>
                         !string.Equals(
                             b.Status,
                             "Paid",
                             StringComparison.OrdinalIgnoreCase)))
            {
                var verifiedAmount =
                    await _context.Payments
                        .Where(p =>
                            p.BillId == bill.BillId &&
                            p.Status == "Paid")
                        .SumAsync(p =>
                            (decimal?)p.Amount)
                    ?? 0m;

                var remaining =
                    bill.TotalAmount -
                    verifiedAmount;

                if (remaining > 0)
                {
                    outstandingAmount +=
                        remaining;
                }
            }


            // -----------------------------------------------------
            // LATEST PAYMENT
            // -----------------------------------------------------

            var latestPayment =
                await _context.Payments
                    .AsNoTracking()
                    .Where(p =>
                        p.CustomerId ==
                        customer.CustomerId)
                    .OrderByDescending(p =>
                        p.PaymentId)
                    .FirstOrDefaultAsync();


            // -----------------------------------------------------
            // CUSTOMER-FACING CONNECTION STATUS
            // -----------------------------------------------------

            var customerFacingConnectionStatus =
                connection?.Status
                ?? "No Connection";

            if (latestPayment != null &&
                (
                    string.Equals(
                        latestPayment.Status,
                        "Pending Verification",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                        latestPayment.Status,
                        "Pending",
                        StringComparison.OrdinalIgnoreCase)
                ))
            {
                customerFacingConnectionStatus =
                    "Payment Verification Pending";
            }


            // -----------------------------------------------------
            // RECENT APPLICATIONS
            // -----------------------------------------------------

            var recentApplications =
                await _context.Orders
                    .AsNoTracking()
                    .Include(o => o.Plan)
                    .Where(o =>
                        o.CustomerId ==
                        customer.CustomerId)
                    .OrderByDescending(o =>
                        o.OrderId)
                    .Take(5)
                    .ToListAsync();


            // -----------------------------------------------------
            // BUILD VIEW MODEL
            // -----------------------------------------------------

            var model =
                new CustomerDashboardViewModel
                {
                    Customer =
                        customer,

                    TotalApplications =
                        totalApplications,

                    Connection =
                        connection,

                    ConnectionStatus =
                        customerFacingConnectionStatus,

                    OutstandingAmount =
                        outstandingAmount,

                    AccountStatus =
                        customer.IsActive
                            ? "Active"
                            : "Inactive",

                    LatestBill =
                        bills.FirstOrDefault(),

                    LatestPayment =
                        latestPayment,

                    RecentApplications =
                        recentApplications
                };


            return View(model);
        }


        // =========================================================
        // APPLY FOR NEW CONNECTION - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            var customer =
                await GetLoggedInCustomerAsync();

            if (customer == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            await LoadApplyDataAsync(customer);


            return View(
                new CustomerApplyViewModel());
        }


        // =========================================================
        // APPLY FOR NEW CONNECTION - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(
            CustomerApplyViewModel model)
        {
            var customer =
                await GetLoggedInCustomerAsync();

            if (customer == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (!ModelState.IsValid)
            {
                await LoadApplyDataAsync(customer);

                return View(model);
            }


            // -----------------------------------------------------
            // NORMALIZE CONNECTION TYPE
            // -----------------------------------------------------

            var requestedType =
                NormalizeConnectionType(
                    model.ConnectionType);


            if (string.IsNullOrEmpty(
                requestedType))
            {
                ModelState.AddModelError(
                    "ConnectionType",
                    "Please select a valid connection type.");

                await LoadApplyDataAsync(customer);

                return View(model);
            }


            // -----------------------------------------------------
            // FIND SELECTED ACTIVE PLAN
            // -----------------------------------------------------

            var plan =
                await _context.Plans
                    .FirstOrDefaultAsync(p =>
                        p.PlanId ==
                            model.PlanId &&
                        p.IsActive);


            if (plan == null)
            {
                ModelState.AddModelError(
                    "PlanId",
                    "Selected plan is not available.");

                await LoadApplyDataAsync(customer);

                return View(model);
            }


            // -----------------------------------------------------
            // VALIDATE PLAN CONNECTION TYPE
            // -----------------------------------------------------

            var planType =
                NormalizeConnectionType(
                    plan.ConnectionType);


            if (requestedType != planType)
            {
                ModelState.AddModelError(
                    "PlanId",
                    "Selected plan does not belong to the selected connection type.");

                await LoadApplyDataAsync(customer);

                return View(model);
            }


            // -----------------------------------------------------
            // CREATE ORDER
            // -----------------------------------------------------

            var order =
                new Order
                {
                    OrderNumber =
                        $"TMP{Guid.NewGuid():N}"
                            .Substring(0, 20),

                    CustomerId =
                        customer.CustomerId,

                    ConnectionType =
                        requestedType,

                    PlanId =
                        plan.PlanId,

                    Equipment =
                        string.IsNullOrWhiteSpace(
                            model.Equipment)
                            ? null
                            : model.Equipment.Trim(),

                    AdditionalInfo =
                        string.IsNullOrWhiteSpace(
                            model.AdditionalInfo)
                            ? null
                            : model.AdditionalInfo.Trim(),

                    Status =
                        "Pending",

                    CreatedAt =
                        DateTime.Now
                };


            _context.Orders.Add(order);

            await _context.SaveChangesAsync();


            // -----------------------------------------------------
            // GENERATE FINAL ORDER NUMBER
            // Uses Admin Settings prefixes
            // -----------------------------------------------------

            var prefix =
                await GetOrderPrefixAsync(
                    requestedType);


            order.OrderNumber =
                $"{prefix}{order.OrderId:D6}";

            order.UpdatedAt =
                DateTime.Now;


            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(ApplicationDetails),
                new
                {
                    id = order.OrderId
                });
        }


        // =========================================================
        // MY APPLICATIONS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Applications()
        {
            var customer =
                await GetLoggedInCustomerAsync();


            if (customer == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var applications =
                await _context.Orders
                    .Include(o => o.Plan)
                    .Where(o =>
                        o.CustomerId ==
                        customer.CustomerId)
                    .OrderByDescending(o =>
                        o.OrderId)
                    .ToListAsync();


            return View(applications);
        }


        // =========================================================
        // APPLICATION DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ApplicationDetails(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var customer =
                await GetLoggedInCustomerAsync();


            if (customer == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var application =
                await _context.Orders
                    .Include(o => o.Plan)
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o =>
                        o.OrderId == id &&
                        o.CustomerId ==
                            customer.CustomerId);


            if (application == null)
            {
                return NotFound();
            }


            return View(application);
        }


        // =========================================================
        // CONNECTION INFORMATION
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Connection()
        {
            var customer =
                await GetLoggedInCustomerAsync();


            if (customer == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var connection =
                await _context.Connections
                    .Include(c => c.Plan)
                    .Include(c => c.Order)
                    .Where(c =>
                        c.CustomerId ==
                        customer.CustomerId)
                    .OrderByDescending(c =>
                        c.ConnectionId)
                    .FirstOrDefaultAsync();


            return View(connection);
        }


        // =========================================================
        // BILLS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Bills()
        {
            var customer =
                await GetLoggedInCustomerAsync();


            if (customer == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var bills =
                await _context.Bills
                    .Include(b => b.Connection)
                    .Include(b =>
                        b.Connection.Plan)
                    .Where(b =>
                        b.CustomerId ==
                        customer.CustomerId)
                    .OrderByDescending(b =>
                        b.BillId)
                    .ToListAsync();


            return View(bills);
        }


        // =========================================================
        // PAYMENT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Payment(
            int? billId)
        {
            var customer =
                await GetLoggedInCustomerAsync();


            if (customer == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var bills =
                await _context.Bills
                    .Include(b => b.Connection)
                    .Include(b =>
                        b.Connection.Plan)
                    .Where(b =>
                        b.CustomerId ==
                            customer.CustomerId &&
                        b.Status !=
                            "Paid")
                    .OrderBy(b =>
                        b.DueDate)
                    .ToListAsync();


            ViewBag.SelectedBillId =
                billId;


            return View(bills);
        }


        // =========================================================
        // PAYMENT - POST
        // CUSTOMER SUBMITS PAYMENT FOR VERIFICATION
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(
            int billId,
            decimal amount,
            string paymentMethod,
            string? remarks)
        {
            var customer =
                await GetLoggedInCustomerAsync();


            if (customer == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // -----------------------------------------------------
            // FIND CUSTOMER BILL
            // -----------------------------------------------------

            var bill =
                await _context.Bills
                    .FirstOrDefaultAsync(b =>
                        b.BillId ==
                            billId &&
                        b.CustomerId ==
                            customer.CustomerId);


            if (bill == null)
            {
                return NotFound();
            }


            // -----------------------------------------------------
            // BILL ALREADY PAID
            // -----------------------------------------------------

            if (string.Equals(
                bill.Status,
                "Paid",
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["PaymentError"] =
                    "This bill has already been paid.";


                return RedirectToAction(
                    nameof(Payment),
                    new
                    {
                        billId
                    });
            }


            // -----------------------------------------------------
            // VALIDATE AMOUNT
            // -----------------------------------------------------

            if (amount <= 0)
            {
                TempData["PaymentError"] =
                    "Please enter a valid payment amount.";


                return RedirectToAction(
                    nameof(Payment),
                    new
                    {
                        billId
                    });
            }


            // -----------------------------------------------------
            // VALIDATE PAYMENT METHOD
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                paymentMethod))
            {
                TempData["PaymentError"] =
                    "Please select a payment method.";


                return RedirectToAction(
                    nameof(Payment),
                    new
                    {
                        billId
                    });
            }


            // -----------------------------------------------------
            // CALCULATE VERIFIED AMOUNT
            // -----------------------------------------------------

            var verifiedAmount =
                await _context.Payments
                    .Where(p =>
                        p.BillId ==
                            bill.BillId &&
                        p.Status ==
                            "Paid")
                    .SumAsync(p =>
                        (decimal?)p.Amount)
                    ?? 0m;


            var remainingAmount =
                bill.TotalAmount -
                verifiedAmount;


            // -----------------------------------------------------
            // BILL ALREADY FULLY COVERED
            // -----------------------------------------------------

            if (remainingAmount <= 0)
            {
                TempData["PaymentError"] =
                    "This bill has already been fully paid.";


                return RedirectToAction(
                    nameof(Payment),
                    new
                    {
                        billId
                    });
            }


            // -----------------------------------------------------
            // PREVENT OVERPAYMENT
            // -----------------------------------------------------

            if (amount > remainingAmount)
            {
                TempData["PaymentError"] =
                    $"Payment amount cannot exceed the outstanding amount of Rs. {remainingAmount:N2}.";


                return RedirectToAction(
                    nameof(Payment),
                    new
                    {
                        billId
                    });
            }


            // -----------------------------------------------------
            // PREVENT DUPLICATE PENDING PAYMENT
            // -----------------------------------------------------

            var pendingPaymentExists =
                await _context.Payments
                    .AnyAsync(p =>
                        p.BillId ==
                            bill.BillId &&
                        (
                            p.Status ==
                                "Pending Verification" ||
                            p.Status ==
                                "Pending"
                        ));


            if (pendingPaymentExists)
            {
                TempData["PaymentError"] =
                    "A payment for this bill is already pending verification. Please wait for Accounts approval.";


                return RedirectToAction(
                    nameof(Payment),
                    new
                    {
                        billId
                    });
            }


            // -----------------------------------------------------
            // CREATE PAYMENT
            // -----------------------------------------------------

            var payment =
                new Payment
                {
                    PaymentNumber =
                        $"PAY{Guid.NewGuid():N}"
                            .Substring(0, 20),

                    CustomerId =
                        customer.CustomerId,

                    BillId =
                        bill.BillId,

                    AccountId =
                        bill.AccountId,

                    Amount =
                        amount,

                    PaymentMethod =
                        paymentMethod.Trim(),

                    Status =
                        "Pending Verification",

                    PaymentDate =
                        DateTime.Now,

                    Remarks =
                        string.IsNullOrWhiteSpace(
                            remarks)
                            ? null
                            : remarks.Trim()
                };


            _context.Payments.Add(payment);


            /*
             * Customer only submits payment.
             *
             * Accounts verification later handles:
             *
             * Payment    -> Paid
             * Bill       -> Paid / Partially Paid
             * Order      -> Payment Verified / Payment Pending
             * Connection -> Payment Verified / Payment Pending
             */

            await _context.SaveChangesAsync();


            // -----------------------------------------------------
            // FINAL PAYMENT NUMBER
            // -----------------------------------------------------

            payment.PaymentNumber =
                $"PAY{payment.PaymentId:D6}";


            await _context.SaveChangesAsync();


            // -----------------------------------------------------
            // SUCCESS
            // -----------------------------------------------------

            TempData["PaymentSuccess"] =
                $"Payment {payment.PaymentNumber} submitted successfully and is pending Accounts verification.";


            return RedirectToAction(
                nameof(Payment),
                new
                {
                    billId
                });
        }


        // =========================================================
        // FEEDBACK - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Feedback()
        {
            var customer =
                await GetLoggedInCustomerAsync();


            if (customer == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // -----------------------------------------------------
            // CHECK ADMIN FEEDBACK SETTING
            // -----------------------------------------------------

            var settings =
                await GetSystemSettingsAsync();


            if (!settings.FeedbackEnabled)
            {
                TempData["FeedbackDisabled"] =
                    "Customer feedback is currently unavailable.";

                return RedirectToAction(
                    nameof(Index));
            }


            var feedbacks =
                await _context.Feedbacks
                    .Where(f =>
                        f.CustomerId ==
                            customer.CustomerId)
                    .OrderByDescending(f =>
                        f.FeedbackId)
                    .ToListAsync();


            return View(feedbacks);
        }


        // =========================================================
        // FEEDBACK - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Feedback(
            string subject,
            string message,
            int rating)
        {
            var customer =
                await GetLoggedInCustomerAsync();


            if (customer == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // -----------------------------------------------------
            // CHECK ADMIN FEEDBACK SETTING AGAIN
            // -----------------------------------------------------

            /*
             * Server-side enforcement.
             *
             * Even if somebody manually sends a POST request,
             * feedback cannot be created while the Administrator
             * has disabled the feature.
             */
            var settings =
                await GetSystemSettingsAsync();


            if (!settings.FeedbackEnabled)
            {
                TempData["FeedbackDisabled"] =
                    "Customer feedback is currently unavailable.";

                return RedirectToAction(
                    nameof(Index));
            }


            // -----------------------------------------------------
            // SUBJECT
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                subject))
            {
                TempData["FeedbackError"] =
                    "Please select a feedback subject.";


                return RedirectToAction(
                    nameof(Feedback));
            }


            // -----------------------------------------------------
            // MESSAGE
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                message))
            {
                TempData["FeedbackError"] =
                    "Please enter your feedback message.";


                return RedirectToAction(
                    nameof(Feedback));
            }


            // -----------------------------------------------------
            // RATING
            // -----------------------------------------------------

            if (rating < 1 ||
                rating > 5)
            {
                TempData["FeedbackError"] =
                    "Please select a rating between 1 and 5.";


                return RedirectToAction(
                    nameof(Feedback));
            }


            // -----------------------------------------------------
            // CREATE FEEDBACK
            // -----------------------------------------------------

            var feedback =
                new Feedback
                {
                    CustomerId =
                        customer.CustomerId,

                    Subject =
                        subject.Trim(),

                    Message =
                        message.Trim(),

                    Rating =
                        rating,

                    Status =
                        "Submitted",

                    CreatedAt =
                        DateTime.Now
                };


            _context.Feedbacks.Add(
                feedback);


            await _context.SaveChangesAsync();


            TempData["FeedbackSuccess"] =
                "Thank you! Your feedback has been submitted successfully.";


            return RedirectToAction(
                nameof(Feedback));
        }


        // =========================================================
        // GET LOGGED-IN CUSTOMER
        // =========================================================

        private async Task<Customer?>
            GetLoggedInCustomerAsync()
        {
            var user =
                await _userManager
                    .GetUserAsync(User);


            if (user == null)
            {
                return null;
            }


            return await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.IdentityUserId ==
                        user.Id &&
                    c.IsActive);
        }


        // =========================================================
        // LOAD APPLY PAGE DATA
        // =========================================================

        private async Task LoadApplyDataAsync(
            Customer customer)
        {
            ViewBag.Customer =
                customer;


            ViewBag.Plans =
                await _context.Plans
                    .Where(p =>
                        p.IsActive)
                    .OrderBy(p =>
                        p.ConnectionType)
                    .ThenBy(p =>
                        p.Price)
                    .ToListAsync();
        }


        // =========================================================
        // NORMALIZE CONNECTION TYPE
        // =========================================================

        private static string NormalizeConnectionType(
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return string.Empty;
            }


            var type =
                value
                    .Trim()
                    .ToLowerInvariant();


            if (type.Contains(
                "broadband"))
            {
                return "Broadband Internet";
            }


            if (type.Contains(
                "dial"))
            {
                return "Dial-Up Internet";
            }


            if (type.Contains("telephone") ||
                type.Contains("landline"))
            {
                return "Telephone/Landline";
            }


            return string.Empty;
        }


        // =========================================================
        // ORDER NUMBER PREFIX
        // Uses Admin System Settings
        // =========================================================

        private async Task<string> GetOrderPrefixAsync(
            string connectionType)
        {
            var settings =
                await GetSystemSettingsAsync();


            return connectionType switch
            {
                "Broadband Internet" =>
                    string.IsNullOrWhiteSpace(
                        settings.BroadbandPrefix)
                        ? "B"
                        : settings.BroadbandPrefix
                            .Trim()
                            .ToUpperInvariant(),

                "Dial-Up Internet" =>
                    string.IsNullOrWhiteSpace(
                        settings.DialUpPrefix)
                        ? "D"
                        : settings.DialUpPrefix
                            .Trim()
                            .ToUpperInvariant(),

                "Telephone/Landline" =>
                    string.IsNullOrWhiteSpace(
                        settings.TelephonePrefix)
                        ? "T"
                        : settings.TelephonePrefix
                            .Trim()
                            .ToUpperInvariant(),

                _ =>
                    "O"
            };
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


            if (settings != null)
            {
                return settings;
            }


            /*
             * Safe fallback values.
             *
             * Existing customer functionality remains operational
             * if the settings record is unexpectedly unavailable.
             */
            return new SystemSetting
            {
                ApplicationName =
                    "NEXUS",

                SupportEmail =
                    "support@nexus.com",

                SupportPhone =
                    "",

                CompanyAddress =
                    "",

                TaxPercentage =
                    12.24m,

                DefaultBillDueDays =
                    10,

                Currency =
                    "PKR",

                CityCode =
                    "101",

                BroadbandPrefix =
                    "B",

                TelephonePrefix =
                    "T",

                DialUpPrefix =
                    "D",

                CustomerRegistrationEnabled =
                    true,

                FeedbackEnabled =
                    true,

                MaintenanceMode =
                    false
            };
        }
    }
}