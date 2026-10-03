using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;
using NEXUS_eProject.Services;

namespace NEXUS_eProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<HomeController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }


        // =========================================================
        // HOME
        // =========================================================

        public IActionResult Index()
        {
            return View();
        }


        // =========================================================
        // ABOUT
        // =========================================================

        public IActionResult About()
        {
            return View();
        }


        // =========================================================
        // SERVICES
        // =========================================================

        public IActionResult Services()
        {
            return View();
        }


        // =========================================================
        // PLANS
        // =========================================================

        public IActionResult Plans()
        {
            return View();
        }


        // =========================================================
        // CONTACT - GET
        // =========================================================

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactFormViewModel());
        }


        // =========================================================
        // CONTACT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(
            ContactFormViewModel model)
        {
            // -----------------------------------------------------
            // SERVER-SIDE VALIDATION
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // -----------------------------------------------------
            // SEND EMAIL
            // -----------------------------------------------------

            try
            {
                await _emailService.SendContactEmailAsync(model);

                TempData["ContactSuccess"] =
                    "Your message has been sent successfully. Our team will get back to you soon.";

                return RedirectToAction(nameof(Contact));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while sending a NEXUS contact form email.");

                ModelState.AddModelError(
                    string.Empty,
                    "We could not send your message right now. Please try again shortly.");

                return View(model);
            }
        }


        // =========================================================
        // FAQ + APPROVED CUSTOMER FEEDBACK
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Faq()
        {
            var feedbacks =
                await _context.Feedbacks
                    .Include(f => f.Customer)
                    .Where(f =>
                        f.Status == "Approved" &&
                        f.Rating >= 1 &&
                        f.Rating <= 5)
                    .OrderByDescending(f =>
                        f.CreatedAt)
                    .Take(12)
                    .ToListAsync();


            return View(feedbacks);
        }


        // =========================================================
        // MAINTENANCE PAGE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Maintenance()
        {
            /*
             * We load System Settings here so that the maintenance
             * page can display the current NEXUS application name,
             * support email and support phone configured by Admin.
             */

            var settings =
                await _context.SystemSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync();


            /*
             * If Maintenance Mode is OFF, users should not normally
             * stay on the maintenance page.
             *
             * Redirect them back to the public website.
             */

            if (settings == null ||
                !settings.MaintenanceMode)
            {
                return RedirectToAction(
                    nameof(Index));
            }


            return View(settings);
        }
    }
}