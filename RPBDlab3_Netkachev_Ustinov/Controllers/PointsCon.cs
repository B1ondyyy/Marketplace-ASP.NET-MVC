using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using RPBDlab3_Netkachev_Ustinov.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RPBDlab3_Netkachev_Ustinov.Controllers
{
    public class PickupPointsController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<ObserverUpdater> _hubContext;

        public PickupPointsController(DatabaseContext context, IHubContext<ObserverUpdater> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string searchString, string searchColumn, string sortOrder)
        {
            ViewData["AddressSortParam"] = string.IsNullOrEmpty(sortOrder) ? "address_desc" : "";
            ViewData["CitySortParam"] = sortOrder == "city" ? "city_desc" : "city";

            var pickupPointsQuery = _context.PickupPoints.Include(p => p.City).AsQueryable();

            pickupPointsQuery = sortOrder switch
            {
                "address_desc" => pickupPointsQuery.OrderByDescending(p => p.PickupPointAddress),
                "city" => pickupPointsQuery.OrderBy(p => p.City.CityName),
                "city_desc" => pickupPointsQuery.OrderByDescending(p => p.City.CityName),
                _ => pickupPointsQuery.OrderBy(p => p.PickupPointAddress),
            };

            if (!string.IsNullOrEmpty(searchString))
            {
                pickupPointsQuery = searchColumn switch
                {
                    "address" => pickupPointsQuery.Where(s => s.PickupPointAddress.ToLower().Contains(searchString.ToLower())),
                    "city" => pickupPointsQuery.Where(s => s.City.CityName.ToLower().Contains(searchString.ToLower())),
                    _ => pickupPointsQuery,
                };
            }

            return View(await pickupPointsQuery.ToListAsync());
        }

        private async Task<IActionResult> GetPickupPointViewById(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pickupPoints = await _context.PickupPoints.Include(p => p.City).FirstOrDefaultAsync(m => m.Id == id);

            return pickupPoints == null ? NotFound() : View(pickupPoints);
        }

        // GET: Create
        public IActionResult Create()
        {
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "CityName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        // POST: Create
        public async Task<IActionResult> Create([Bind("Id,PickupPointAddress,CityId")] PickupPoints pickupPoints)
        {
            return await ProcessPickupPointEdit(pickupPoints, ModelState.IsValid);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pickupPoints = await _context.PickupPoints
                .Include(p => p.City)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pickupPoints == null)
            {
                return NotFound();
            }

            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "CityName", pickupPoints.CityId);

            return View(pickupPoints);
        }

        // Post: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PickupPointAddress,CityId")] PickupPoints pickupPoints)
        {
            if (id != pickupPoints.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pickupPoints);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PickupPointsExists(pickupPoints.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", pickupPoints.Id);
                return RedirectToAction(nameof(Index));
            }

            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "CityName", pickupPoints.CityId);

            return View(pickupPoints);
        }

        // Get: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            return await GetPickupPointViewById(id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        // Post: Delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            return await ProcessPickupPointDelete(id);
        }

        private async Task<IActionResult> ProcessPickupPointEdit(PickupPoints pickupPoints, bool isValid, int? id = null)
        {
            if (!isValid)
            {
                return View(pickupPoints);
            }

            if (id != null && id != pickupPoints.Id)
            {
                return NotFound();
            }

            try
            {
                if (id == null)
                {
                    _context.Add(pickupPoints);
                }
                else
                {
                    _context.Update(pickupPoints);
                }

                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", pickupPoints.Id);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PickupPointsExists(pickupPoints.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<IActionResult> ProcessPickupPointDelete(int id)
        {
            var pickupPoints = await _context.PickupPoints.FindAsync(id);

            if (pickupPoints != null)
            {
                _context.PickupPoints.Remove(pickupPoints);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", pickupPoints.Id);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PickupPointsExists(int id)
        {
            return _context.PickupPoints.Any(e => e.Id == id);
        }
    }
}