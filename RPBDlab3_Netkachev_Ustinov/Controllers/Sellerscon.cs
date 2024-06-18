using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using RPBDlab3_Netkachev_Ustinov.Models;

namespace RPBDlab3_Netkachev_Ustinov.Controllers
{
    public class SellersController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<ObserverUpdater> _hubContext;

        public SellersController(DatabaseContext context, IHubContext<ObserverUpdater> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string searchString, string searchColumn, string sortOrder)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CompanySortParm"] = sortOrder == "company" ? "company_desc" : "company";

            IQueryable<Sellers> sellersQuery = _context.Sellers.Include(s => s.Company);

            // Поиск
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                sellersQuery = sellersQuery.Where(s =>
                    s.SellerName.ToLower().Contains(searchString) ||
                    s.Company.CompanyName.ToLower().Contains(searchString)
                );
            }

            // Сортировка
            sellersQuery = sortOrder switch
            {
                "name_desc" => sellersQuery.OrderByDescending(s => s.SellerName),
                "company" => sellersQuery.OrderBy(s => s.Company.CompanyName),
                "company_desc" => sellersQuery.OrderByDescending(s => s.Company.CompanyName),
                _ => sellersQuery.OrderBy(s => s.SellerName),
            };

            return View(await sellersQuery.ToListAsync());
        }

        // GET: Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "CompanyName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        // POST: Create
        public async Task<IActionResult> Create([Bind("Id,SellerName,CompanyId")] Sellers sellers)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sellers);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", sellers.Id);
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "CompanyName", sellers.CompanyId);
            return View(sellers);
        }

        // GET: Create
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sellers = await _context.Sellers.FindAsync(id);
            if (sellers == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "CompanyName", sellers.CompanyId);
            return View(sellers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        // POST: Edit
        public async Task<IActionResult> Edit(int id, [Bind("Id,SellerName,CompanyId")] Sellers sellers)
        {
            if (id != sellers.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sellers);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SellersExists(sellers.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", sellers.Id);
                return RedirectToAction(nameof(Index));
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "CompanyName", sellers.CompanyId);
            return View(sellers);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sellers = await _context.Sellers
                .Include(s => s.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sellers == null)
            {
                return NotFound();
            }

            return View(sellers);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        // POST: Delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sellers = await _context.Sellers.FindAsync(id);
            if (sellers != null)
            {
                _context.Sellers.Remove(sellers);
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("SendUpdateNotification", sellers.Id);
            return RedirectToAction(nameof(Index));
        }

        private bool SellersExists(int id)
        {
            return _context.Sellers.Any(e => e.Id == id);
        }
    }
}
