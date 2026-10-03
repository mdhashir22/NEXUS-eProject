using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Accounts Employee")]
    public class AccountsPaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsPaymentController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // ALL PAYMENTS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status)
        {
            var query = _context.Payments
                .AsNoTracking()
                .Include(p => p.Customer)
                .Include(p => p.Bill)
                .AsQueryable();


            // =================================================
            // SEARCH
            // =================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.PaymentNumber.Contains(search) ||
                    p.AccountId.Contains(search) ||

                    (p.Customer != null &&
                     p.Customer.FullName.Contains(search)) ||

                    (p.Bill != null &&
                     p.Bill.BillNumber.Contains(search))
                );
            }


            // =================================================
            // STATUS FILTER
            // =================================================

            if (!string.IsNullOrWhiteSpace(status) &&
                status != "All")
            {
                query = query.Where(p =>
                    p.Status == status);
            }


            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();


            ViewBag.Search = search;
            ViewBag.Status = status;


            return View(payments);
        }


        // =====================================================
        // PENDING PAYMENT VERIFICATIONS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Pending(
            string? search)
        {
            var query = _context.Payments
                .AsNoTracking()
                .Include(p => p.Customer)
                .Include(p => p.Bill)
                .Where(p =>
                    p.Status == "Pending Verification" ||
                    p.Status == "Pending")
                .AsQueryable();


            // =================================================
            // SEARCH
            // =================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.PaymentNumber.Contains(search) ||
                    p.AccountId.Contains(search) ||

                    (p.Customer != null &&
                     p.Customer.FullName.Contains(search)) ||

                    (p.Bill != null &&
                     p.Bill.BillNumber.Contains(search))
                );
            }


            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();


            ViewBag.Search = search;


            return View(payments);
        }


        // =====================================================
        // PAYMENT DETAILS / REVIEW
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _context.Payments
                .AsNoTracking()

                .Include(p => p.Customer)

                .Include(p => p.Bill)
                    .ThenInclude(b => b.Connection)
                        .ThenInclude(c => c.Plan)

                .Include(p => p.Bill)
                    .ThenInclude(b => b.Connection)
                        .ThenInclude(c => c.Order)

                .FirstOrDefaultAsync(p =>
                    p.PaymentId == id);


            if (payment == null)
            {
                return NotFound();
            }


            // =================================================
            // PREVIOUS VERIFIED AMOUNT
            // =================================================

            var verifiedAmount =
                await _context.Payments
                    .Where(p =>
                        p.BillId == payment.BillId &&
                        p.PaymentId != payment.PaymentId &&
                        p.Status == "Paid")
                    .SumAsync(p =>
                        (decimal?)p.Amount) ?? 0m;


            ViewBag.VerifiedAmount =
                verifiedAmount;


            return View(payment);
        }


        // =====================================================
        // VERIFY CUSTOMER PAYMENT
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(int id)
        {
            var payment = await _context.Payments

                .Include(p => p.Bill)

                    .ThenInclude(b => b.Connection)

                        .ThenInclude(c => c.Order)

                .FirstOrDefaultAsync(p =>
                    p.PaymentId == id);


            if (payment == null)
            {
                return NotFound();
            }


            // =================================================
            // ONLY PENDING PAYMENT CAN BE VERIFIED
            // =================================================

            if (payment.Status != "Pending Verification" &&
                payment.Status != "Pending")
            {
                TempData["Error"] =
                    "This payment has already been processed.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // BILL VALIDATION
            // =================================================

            if (payment.Bill == null)
            {
                TempData["Error"] =
                    "The bill linked with this payment could not be found.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // PAYMENT AMOUNT VALIDATION
            // =================================================

            if (payment.Amount <= 0)
            {
                TempData["Error"] =
                    "Invalid payment amount.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // PREVIOUS VERIFIED PAYMENTS
            // =================================================

            var previousVerifiedAmount =
                await _context.Payments
                    .Where(p =>
                        p.BillId == payment.BillId &&
                        p.PaymentId != payment.PaymentId &&
                        p.Status == "Paid")
                    .SumAsync(p =>
                        (decimal?)p.Amount) ?? 0m;


            // =================================================
            // REMAINING BALANCE BEFORE CURRENT PAYMENT
            // =================================================

            var remainingBeforeVerification =
                payment.Bill.TotalAmount -
                previousVerifiedAmount;


            if (remainingBeforeVerification <= 0)
            {
                TempData["Error"] =
                    "This bill has already been fully paid.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // PREVENT OVERPAYMENT
            // =================================================

            if (payment.Amount >
                remainingBeforeVerification)
            {
                TempData["Error"] =
                    $"This payment exceeds the remaining bill balance of Rs. {remainingBeforeVerification:N2}.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // TOTAL VERIFIED AMOUNT
            // =================================================

            var totalVerifiedAmount =
                previousVerifiedAmount +
                payment.Amount;


            // =================================================
            // VERIFY CURRENT PAYMENT
            // =================================================

            payment.Status =
                "Paid";


            // =================================================
            // FULL BILL PAID
            // =================================================

            if (totalVerifiedAmount >=
                payment.Bill.TotalAmount)
            {
                payment.Bill.Status =
                    "Paid";


                // ---------------------------------------------
                // CONNECTION
                // ---------------------------------------------

                if (payment.Bill.Connection != null)
                {
                    payment.Bill.Connection.Status =
                        "Payment Verified";
                }


                // ---------------------------------------------
                // ORDER
                //
                // TechnicalInstallationController expects
                // Payment Verified.
                // ---------------------------------------------

                if (payment.Bill.Connection?.Order != null)
                {
                    payment.Bill.Connection.Order.Status =
                        "Payment Verified";

                    payment.Bill.Connection.Order.UpdatedAt =
                        DateTime.Now;
                }


                TempData["Success"] =
                    "Payment verified successfully. Bill is fully paid and the connection is ready for Technical installation.";
            }

            // =================================================
            // PARTIAL PAYMENT
            // =================================================

            else
            {
                payment.Bill.Status =
                    "Partially Paid";


                // ---------------------------------------------
                // CONNECTION
                // ---------------------------------------------

                if (payment.Bill.Connection != null)
                {
                    payment.Bill.Connection.Status =
                        "Payment Pending";
                }


                // ---------------------------------------------
                // ORDER
                // ---------------------------------------------

                if (payment.Bill.Connection?.Order != null)
                {
                    payment.Bill.Connection.Order.Status =
                        "Payment Pending";

                    payment.Bill.Connection.Order.UpdatedAt =
                        DateTime.Now;
                }


                TempData["Success"] =
                    "Payment verified successfully. The bill is partially paid and still has an outstanding balance.";
            }


            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(Details),
                new { id });
        }


        // =====================================================
        // REJECT CUSTOMER PAYMENT
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            int id,
            string? rejectionReason)
        {
            var payment = await _context.Payments

                .Include(p => p.Bill)

                    .ThenInclude(b => b.Connection)

                        .ThenInclude(c => c.Order)

                .FirstOrDefaultAsync(p =>
                    p.PaymentId == id);


            if (payment == null)
            {
                return NotFound();
            }


            // =================================================
            // ONLY PENDING PAYMENT CAN BE REJECTED
            // =================================================

            if (payment.Status != "Pending Verification" &&
                payment.Status != "Pending")
            {
                TempData["Error"] =
                    "This payment has already been processed.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // REJECTION REASON REQUIRED
            // =================================================

            if (string.IsNullOrWhiteSpace(
                rejectionReason))
            {
                TempData["Error"] =
                    "Please enter a reason for rejecting the payment.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            // =================================================
            // REJECT PAYMENT
            // =================================================

            payment.Status =
                "Rejected";


            var reason =
                rejectionReason.Trim();


            if (string.IsNullOrWhiteSpace(
                payment.Remarks))
            {
                payment.Remarks =
                    $"Rejected: {reason}";
            }
            else
            {
                payment.Remarks =
                    $"{payment.Remarks} | Rejected: {reason}";
            }


            // =================================================
            // RECALCULATE BILL USING VERIFIED PAYMENTS ONLY
            // =================================================

            if (payment.Bill != null)
            {
                var verifiedAmount =
                    await _context.Payments

                        .Where(p =>
                            p.BillId ==
                                payment.BillId &&

                            p.PaymentId !=
                                payment.PaymentId &&

                            p.Status ==
                                "Paid")

                        .SumAsync(p =>
                            (decimal?)p.Amount) ?? 0m;


                // =============================================
                // FULLY PAID
                // =============================================

                if (verifiedAmount >=
                    payment.Bill.TotalAmount)
                {
                    payment.Bill.Status =
                        "Paid";


                    if (payment.Bill.Connection != null)
                    {
                        payment.Bill.Connection.Status =
                            "Payment Verified";
                    }


                    if (payment.Bill.Connection?.Order != null)
                    {
                        payment.Bill.Connection.Order.Status =
                            "Payment Verified";

                        payment.Bill.Connection.Order.UpdatedAt =
                            DateTime.Now;
                    }
                }

                // =============================================
                // PARTIALLY PAID
                // =============================================

                else if (verifiedAmount > 0)
                {
                    payment.Bill.Status =
                        "Partially Paid";


                    if (payment.Bill.Connection != null)
                    {
                        payment.Bill.Connection.Status =
                            "Payment Pending";
                    }


                    if (payment.Bill.Connection?.Order != null)
                    {
                        payment.Bill.Connection.Order.Status =
                            "Payment Pending";

                        payment.Bill.Connection.Order.UpdatedAt =
                            DateTime.Now;
                    }
                }

                // =============================================
                // NOTHING VERIFIED
                // =============================================

                else
                {
                    payment.Bill.Status =
                        "Unpaid";


                    if (payment.Bill.Connection != null)
                    {
                        payment.Bill.Connection.Status =
                            "Payment Pending";
                    }


                    if (payment.Bill.Connection?.Order != null)
                    {
                        payment.Bill.Connection.Order.Status =
                            "Payment Pending";

                        payment.Bill.Connection.Order.UpdatedAt =
                            DateTime.Now;
                    }
                }
            }


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Payment rejected successfully.";


            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }
}