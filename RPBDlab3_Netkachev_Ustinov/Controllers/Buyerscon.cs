using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using RPBDlab3_Netkachev_Ustinov.Models;

namespace RPBDlab3_Netkachev_Ustinov.Controllers
{
    namespace RPBDlab3_Netkachev_Ustinov.Controllers
    {
        public class CustomersController : Controller
        {
            private readonly DatabaseContext _context;
            private readonly IHubContext<ObserverUpdater> _hubContext;

            public CustomersController(DatabaseContext context, IHubContext<ObserverUpdater> hubContext)
            {
                _context = context;
                _hubContext = hubContext;
            }

            public async Task<IActionResult> Index(string searchString, string searchColumn, string sortOrder)
            {
                ViewData["CustomerNameSortParam"] = String.IsNullOrEmpty(sortOrder) ? "customerName_desc" : "";
                ViewData["BirthDateSortParam"] = sortOrder == "BirthDate" ? "birthDate_desc" : "BirthDate";
                ViewData["CitySortParam"] = sortOrder == "City" ? "city_desc" : "City";

                IQueryable<Customers> customersQuery = _context.Customers.Include(c => c.City);

                customersQuery = sortOrder switch
                {
                    "customerName_desc" => customersQuery.OrderByDescending(c => c.CustomerName),
                    "BirthDate" => customersQuery.OrderBy(c => c.BirthDate),
                    "birthDate_desc" => customersQuery.OrderByDescending(c => c.BirthDate),
                    "City" => customersQuery.OrderBy(c => c.City.CityName),
                    "city_desc" => customersQuery.OrderByDescending(c => c.City.CityName),
                    _ => customersQuery.OrderBy(c => c.CustomerName),
                };

                if (!String.IsNullOrEmpty(searchString))
                {
                    customersQuery = searchColumn switch
                    {
                        "name" => customersQuery.Where(s => s.CustomerName.ToLower().Contains(searchString.ToLower())),
                        "birthdate" => customersQuery.Where(s => s.BirthDate.ToString().ToLower().Contains(searchString.ToLower())),
                        "city" => customersQuery.Where(s => s.City.CityName.ToLower().Contains(searchString.ToLower())),
                        _ => customersQuery,
                    };
                }

                return View(await customersQuery.ToListAsync());
            }

            // GET: Create
            public IActionResult Create()
            {
                ViewData["CityId"] = new SelectList(_context.Cities, "Id", "CityName");
                return View();
            }

            // POST: Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create([Bind("Id,CustomerName,BirthDate,CityId")] Customers customers)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(customers);
                    await _context.SaveChangesAsync();
                    await _hubContext.Clients.All.SendAsync("SendUpdateNotification", customers.Id);
                    return RedirectToAction(nameof(Index));
                }
                ViewData["CityId"] = new SelectList(_context.Cities, "Id", "CityName", customers.CityId);
                return View(customers);
            }

            // GET: Edit
            public async Task<IActionResult> Edit(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var customers = await _context.Customers.FindAsync(id);
                if (customers == null)
                {
                    return NotFound();
                }
                ViewData["CityId"] = new SelectList(_context.Cities, "Id", "CityName", customers.CityId);
                return View(customers);
            }

            // POST: Edit
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerName,BirthDate,CityId")] Customers customers)
            {
                if (id != customers.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(customers);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CustomersExists(customers.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    await _hubContext.Clients.All.SendAsync("SendUpdateNotification", customers.Id);
                    return RedirectToAction(nameof(Index));
                }
                ViewData["CityId"] = new SelectList(_context.Cities, "Id", "CityName", customers.CityId);
                return View(customers);
            }

            // GET: Delete
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var customers = await _context.Customers
                    .Include(c => c.City)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (customers == null)
                {
                    return NotFound();
                }

                return View(customers);
            }

            // POST: Delete
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var customers = await _context.Customers.FindAsync(id);
                if (customers != null)
                {
                    _context.Customers.Remove(customers);
                }

                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", customers.Id);
                return RedirectToAction(nameof(Index));
            }

            private bool CustomersExists(int id)
            {
                return _context.Customers.Any(e => e.Id == id);
            }
        }
    }
}
