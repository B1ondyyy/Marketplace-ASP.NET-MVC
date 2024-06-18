using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

using RPBDlab3_Netkachev_Ustinov.Models;

namespace RPBDlab3_Netkachev_Ustinov.Controllers
{
    public class BrandsController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<ObserverUpdater> _hubContext;

        public BrandsController(DatabaseContext context, IHubContext<ObserverUpdater> hub)
        {
            _context = context;
            _hubContext = hub;
        }

        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            var brands = _context.Brands.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                brands = brands.Where(s => s.BrandName.ToLower().Contains(searchString.ToLower()));
            }

            brands = sortOrder switch
            {
                "name_desc" => brands.OrderByDescending(b => b.BrandName),
                _ => brands.OrderBy(b => b.BrandName),
            };

            return View(await brands.ToListAsync());
        }
        //GET: CREATE
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]

        //POST: CREATE
        public async Task<IActionResult> Create([Bind("Id,BrandName")] Brands brand)
        {
            if (ModelState.IsValid)
            {
                _context.Add(brand);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", brand.Id);
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }
        //GET: EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FindAsync(id);
            return brand == null ? NotFound() : View(brand);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        //POST: EDIT
        public async Task<IActionResult> Edit(int id, [Bind("Id,BrandName")] Brands brand)
        {
            if (id != brand.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(brand);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandsExists(brand.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", brand.Id);
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        //GET: DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FirstOrDefaultAsync(m => m.Id == id);
            return brand == null ? NotFound() : View(brand);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        //POST: DELETE
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                _context.Brands.Remove(brand);
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("SendUpdateNotification", id);
            return RedirectToAction(nameof(Index));
        }

        private bool BrandsExists(int id) => _context.Brands.Any(e => e.Id == id);
    }
}
