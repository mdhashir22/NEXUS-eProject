using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FeedbacksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbacksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Feedbacks
        public async Task<IActionResult> Index()
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Customer)
                .OrderByDescending(f => f.FeedbackId)
                .ToListAsync();

            return View(feedbacks);
        }

        // GET: Feedbacks/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var feedback = await _context.Feedbacks
                .Include(f => f.Customer)
                .FirstOrDefaultAsync(f => f.FeedbackId == id);

            if (feedback == null)
                return NotFound();

            return View(feedback);
        }

        // POST: Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == id);

            if (feedback == null)
                return NotFound();

            feedback.Status = "Approved";
            feedback.RespondedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Feedback approved successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == id);

            if (feedback == null)
                return NotFound();

            feedback.Status = "Rejected";
            feedback.RespondedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Feedback rejected successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Respond
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(
            int id,
            string? adminResponse)
        {
            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == id);

            if (feedback == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(adminResponse))
            {
                TempData["Error"] =
                    "Please enter an admin response.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            feedback.AdminResponse = adminResponse.Trim();
            feedback.RespondedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Response saved successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }
}