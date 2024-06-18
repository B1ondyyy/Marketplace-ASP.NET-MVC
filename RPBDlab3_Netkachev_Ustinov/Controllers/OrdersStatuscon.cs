using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using RPBDlab3_Netkachev_Ustinov.Models;

namespace RPBDlab3_Netkachev_Ustinov.Controllers
{
    public class OrderStatusesController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<ObserverUpdater> _hubContext;

        public OrderStatusesController(DatabaseContext context, IHubContext<ObserverUpdater> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            ViewData["StatusStringSortParam"] = string.IsNullOrEmpty(sortOrder) ? "statusString_desc" : "";

            var orderStatusesQuery = _context.OrderStatuses.AsQueryable();

            orderStatusesQuery = sortOrder switch
            {
                "statusString_desc" => orderStatusesQuery.OrderByDescending(o => o.StatusString),
                _ => orderStatusesQuery.OrderBy(o => o.StatusString),
            };

            if (!string.IsNullOrEmpty(searchString))
            {
                orderStatusesQuery = orderStatusesQuery.Where(s => s.StatusString.ToLower().Contains(searchString.ToLower()));
            }

            return View(await orderStatusesQuery.ToListAsync());
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StatusString")] OrderStatuses orderStatuses)
        {
            return await ProcessOrderStatusEdit(orderStatuses, ModelState.IsValid);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            return await GetOrderStatusViewById(id);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StatusString")] OrderStatuses orderStatuses)
        {
            return await ProcessOrderStatusEdit(orderStatuses, ModelState.IsValid, id);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            return await GetOrderStatusViewById(id);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            return await ProcessOrderStatusDelete(id);
        }

        private async Task<IActionResult> GetOrderStatusViewById(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderStatuses = await _context.OrderStatuses.FirstOrDefaultAsync(m => m.Id == id);

            return orderStatuses == null ? NotFound() : View(orderStatuses);
        }

        private async Task<IActionResult> ProcessOrderStatusEdit(OrderStatuses orderStatuses, bool isValid, int? id = null)
        {
            if (!isValid)
            {
                return View(orderStatuses);
            }

            if (id != null && id != orderStatuses.Id)
            {
                return NotFound();
            }

            try
            {
                if (id == null)
                {
                    _context.Add(orderStatuses);
                }
                else
                {
                    _context.Update(orderStatuses);
                }

                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", orderStatuses.Id);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderStatusesExists(orderStatuses.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<IActionResult> ProcessOrderStatusDelete(int id)
        {
            var orderStatuses = await _context.OrderStatuses.FindAsync(id);

            if (orderStatuses != null)
            {
                _context.OrderStatuses.Remove(orderStatuses);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", orderStatuses.Id);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderStatusesExists(int id)
        {
            return _context.OrderStatuses.Any(e => e.Id == id);
        }
    }
}
