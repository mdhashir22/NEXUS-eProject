using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PlansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Plans
        public async Task<IActionResult> Index()
        {
            var plans = await _context.Plans
                .OrderByDescending(p => p.PlanId)
                .ToListAsync();

            return View(plans);
        }

        // GET: Plans/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Plans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Plan plan)
        {
            if (!ModelState.IsValid)
            {
                return View(plan);
            }

            plan.CreatedAt = DateTime.Now;
            plan.IsActive = true;

            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Plan added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Plans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plan = await _context.Plans.FindAsync(id);

            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        // POST: Plans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Plan plan)
        {
            if (id != plan.PlanId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(plan);
            }

            try
            {
                _context.Update(plan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Plan updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlanExists(plan.PlanId))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Plans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plan = await _context.Plans
                .FirstOrDefaultAsync(p => p.PlanId == id);

            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        // POST: Plans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plan = await _context.Plans.FindAsync(id);

            if (plan != null)
            {
                _context.Plans.Remove(plan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Plan deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PlanExists(int id)
        {
            return _context.Plans.Any(p => p.PlanId == id);
        }
    }
}