using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Vendor)
                .OrderByDescending(p => p.ProductId)
                .ToListAsync();

            return View(products);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            await LoadVendorsAsync();

            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                await LoadVendorsAsync(product.VendorId);
                return View(product);
            }

            var vendorExists = await _context.Vendors
                .AnyAsync(v => v.VendorId == product.VendorId && v.IsActive);

            if (!vendorExists)
            {
                ModelState.AddModelError(
                    "VendorId",
                    "Please select a valid active vendor."
                );

                await LoadVendorsAsync(product.VendorId);
                return View(product);
            }

            product.CreatedAt = DateTime.Now;
            product.IsActive = true;

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Product added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            await LoadVendorsAsync(product.VendorId);

            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] =
                        "Product updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await LoadVendorsAsync(product.VendorId);

            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Product deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadVendorsAsync(int? selectedVendorId = null)
        {
            var vendors = await _context.Vendors
                .Where(v => v.IsActive)
                .OrderBy(v => v.VendorName)
                .ToListAsync();

            ViewBag.Vendors = new SelectList(
                vendors,
                "VendorId",
                "VendorName",
                selectedVendorId
            );
        }

        private bool ProductExists(int id)
        {
            return _context.Products
                .Any(p => p.ProductId == id);
        }
    }
}