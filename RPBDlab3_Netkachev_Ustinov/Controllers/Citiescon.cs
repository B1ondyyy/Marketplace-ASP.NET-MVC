using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPBDlab3_Netkachev_Ustinov.Models;
using Microsoft.AspNetCore.SignalR;

namespace RPBDlab3_Netkachev_Ustinov.Controllers
{
    public class CitiesController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<ObserverUpdater> _hubContext;

        public CitiesController(DatabaseContext context, IHubContext<ObserverUpdater> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewData["CityNameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "cityName_desc" : "";

            IQueryable<Cities> citiesQuery = _context.Cities;

            citiesQuery = sortOrder switch
            {
                "cityName_desc" => citiesQuery.OrderByDescending(c => c.CityName),
                _ => citiesQuery.OrderBy(c => c.CityName),
            };

            if (!string.IsNullOrEmpty(searchString))
            {
                citiesQuery = citiesQuery.Where(s => s.CityName.ToLower().Contains(searchString.ToLower()));
            }

            return View(await citiesQuery.ToListAsync());
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        // POST: Create
        public async Task<IActionResult> Create([Bind("Id,CityName")] Cities city)
        {
            if (ModelState.IsValid)
            {
                _context.Add(city);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", city.Id);
                return RedirectToAction(nameof(Index));
            }

            return View(city);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Edit
        public async Task<IActionResult> Edit(int id, [Bind("Id,CityName")] Cities city)
        {
            if (id != city.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(city);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CitiesExists(city.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", city.Id);
                return RedirectToAction(nameof(Index));
            }

            return View(city);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities.FirstOrDefaultAsync(m => m.Id == id);
            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        // POST: Delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city != null)
            {
                _context.Cities.Remove(city);
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("SendUpdateNotification", city?.Id);
            return RedirectToAction(nameof(Index));
        }

        private bool CitiesExists(int id)
        {
            return _context.Cities.Any(e => e.Id == id);
        }
    }
}
