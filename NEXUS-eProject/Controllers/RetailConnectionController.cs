using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Retail Employee")]
    public class RetailConnectionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RetailConnectionController(ApplicationDbContext context)
        {
            _context = context;
        }

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
                query = query.Where(c => c.Status == status);
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
    }
}