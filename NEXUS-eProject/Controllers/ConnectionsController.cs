using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Data;

namespace NEXUS_eProject.Controllers
{
    [Authorize(Roles = "Admin, Retail Employee, Technical Employee, Accounts Employee")]
    public class ConnectionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConnectionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Connections
        public async Task<IActionResult> Index()
        {
            var connections = await _context.Connections
                .Include(c => c.Customer)
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .OrderByDescending(c => c.ConnectionId)
                .ToListAsync();

            return View(connections);
        }

        // GET: Connections/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var connection = await _context.Connections
                .Include(c => c.Customer)
                .Include(c => c.Plan)
                .Include(c => c.Order)
                .FirstOrDefaultAsync(c => c.ConnectionId == id);

            if (connection == null)
                return NotFound();

            return View(connection);
        }
    }
}