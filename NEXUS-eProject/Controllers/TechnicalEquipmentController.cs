using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Technical Employee")]
    public class TechnicalEquipmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TechnicalEquipmentController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // AVAILABLE EQUIPMENT / PRODUCTS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? stock)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Vendor)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.ProductName.Contains(search) ||
                    (p.Description != null &&
                     p.Description.Contains(search)) ||
                    (p.Vendor != null &&
                     p.Vendor.VendorName.Contains(search)));
            }

            if (stock == "Available")
            {
                query = query.Where(p => p.Quantity > 0);
            }
            else if (stock == "OutOfStock")
            {
                query = query.Where(p => p.Quantity <= 0);
            }

            var products = await query
                .OrderBy(p => p.ProductName)
                .Select(p => new TechnicalEquipmentViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    VendorName = p.Vendor != null
                        ? p.Vendor.VendorName
                        : "N/A",
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Description = p.Description ?? string.Empty,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Stock = stock;

            return View(products);
        }


        // =====================================================
        // EQUIPMENT ASSIGNMENTS
        //
        // Existing Connection.Equipment + RouterSerial are used
        // as the current equipment assignment record.
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Assignments(
            string? search)
        {
            var query = _context.Connections
                .AsNoTracking()
                .Include(c => c.Customer)
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .Where(c =>
                    !string.IsNullOrWhiteSpace(c.Equipment) ||
                    !string.IsNullOrWhiteSpace(c.RouterSerial))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.AccountId.Contains(search) ||
                    (c.Customer != null &&
                     c.Customer.FullName.Contains(search)) ||
                    (c.Equipment != null &&
                     c.Equipment.Contains(search)) ||
                    (c.RouterSerial != null &&
                     c.RouterSerial.Contains(search)));
            }

            var assignments = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;

            return View(assignments);
        }
    }
}