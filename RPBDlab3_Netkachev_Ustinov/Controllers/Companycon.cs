using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using RPBDlab3_Netkachev_Ustinov.Models;

namespace RPBDlab3_Netkachev_Ustinov.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<ObserverUpdater> _hubContext;

        public CompaniesController(DatabaseContext context, IHubContext<ObserverUpdater> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewData["CompanyNameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "companyName_desc" : "";

            IQueryable<Companies> companiesQuery = _context.Companies;

            companiesQuery = sortOrder switch
            {
                "companyName_desc" => companiesQuery.OrderByDescending(c => c.CompanyName),
                _ => companiesQuery.OrderBy(c => c.CompanyName),
            };

            if (!string.IsNullOrEmpty(searchString))
            {
                companiesQuery = companiesQuery.Where(s => s.CompanyName.ToLower().Contains(searchString.ToLower()));
            }

            return View(await companiesQuery.ToListAsync());
        }


        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyName")] Companies company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", company.Id);
                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName")] Companies company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompaniesExists(company.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", company.Id);

                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("SendUpdateNotification", company?.Id);
            return RedirectToAction(nameof(Index));
        }

        private bool CompaniesExists(int id) => _context.Companies.Any(e => e.Id == id);
    }
}
