using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using RPBDlab3_Netkachev_Ustinov.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Dynamic.Core;

namespace RPBDlab3_Netkachev_Ustinov.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<ObserverUpdater> _hubContext;

        public ProductsController(DatabaseContext context, IHubContext<ObserverUpdater> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string searchString, string searchColumn, string sortOrder)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.BrandSortParm = sortOrder == "brand" ? "brand_desc" : "brand";
            ViewBag.SellerSortParm = sortOrder == "seller" ? "seller_desc" : "seller";
            ViewBag.AmountSortParm = sortOrder == "amount" ? "amount_desc" : "amount";
            ViewBag.WeightSortParm = sortOrder == "weight" ? "weight_desc" : "weight";

            IQueryable<Products> productsQuery = _context.Products.Include(p => p.Brand).Include(p => p.Seller);

            // Поиск
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                productsQuery = productsQuery.Where(p =>
                    p.ProductName.ToLower().Contains(searchString) ||
                    p.Brand.BrandName.ToLower().Contains(searchString) ||
                    p.Seller.SellerName.ToLower().Contains(searchString) ||
                    (searchColumn == "pts" && p.Amount.ToString().Contains(searchString)) ||
                    (searchColumn == "weight" && p.Weight.ToString().Contains(searchString))
                );
            }

            // Сортировка
            productsQuery = sortOrder switch
            {
                "name_desc" => productsQuery.OrderByDescending(p => p.ProductName),
                "brand" => productsQuery.OrderBy(p => p.Brand.BrandName),
                "brand_desc" => productsQuery.OrderByDescending(p => p.Brand.BrandName),
                "seller" => productsQuery.OrderBy(p => p.Seller.SellerName),
                "seller_desc" => productsQuery.OrderByDescending(p => p.Seller.SellerName),
                "amount" => productsQuery.OrderBy(p => p.Amount),
                "amount_desc" => productsQuery.OrderByDescending(p => p.Amount),
                "weight" => productsQuery.OrderBy(p => p.Weight),
                "weight_desc" => productsQuery.OrderByDescending(p => p.Weight),
                _ => productsQuery.OrderBy(p => p.ProductName),
            };

            return View(await productsQuery.ToListAsync());
        }





        // GET: Create
        public IActionResult Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "BrandName");
            ViewData["SellerId"] = new SelectList(_context.Sellers, "Id", "SellerName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Create
        public async Task<IActionResult> Create([Bind("Id,ProductName,Weight,Amount,BrandId,SellerId")] Products product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", product.Id);

                return RedirectToAction(nameof(Index));
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "BrandName", product.BrandId);
            ViewData["SellerId"] = new SelectList(_context.Sellers, "Id", "SellerName", product.SellerId);
            return View(product);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "BrandName", product.BrandId);
            ViewData["SellerId"] = new SelectList(_context.Sellers, "Id", "SellerName", product.SellerId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Edit
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductName,Weight,Amount,BrandId,SellerId")] Products product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductsExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                await _hubContext.Clients.All.SendAsync("SendUpdateNotification", product.Id);
                return RedirectToAction(nameof(Index));
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "BrandName", product.BrandId);
            ViewData["SellerId"] = new SelectList(_context.Sellers, "Id", "SellerName", product.SellerId);
            return View(product);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        // POST: Delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("SendUpdateNotification", product.Id);

            return RedirectToAction(nameof(Index));
        }

        private bool ProductsExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        private static IQueryable<Products> ApplySearchFilter(IQueryable<Products> query, string searchString, string searchColumn)
        {
            return searchColumn.ToLower() switch
            {
                "productname" => query.Where(p => p.ProductName.ToLower().Contains(searchString.ToLower())),
                "brandname" => query.Where(p => p.Brand.BrandName.ToLower().Contains(searchString.ToLower())),
                "seller" => query.Where(p => p.Seller.SellerName.ToLower().Contains(searchString.ToLower())),
                "pts" => Int64.TryParse(searchString, out var pts) ? query.Where(p => p.Amount == pts) : query.Where(p => p.Amount == -1),
                "weight" => Double.TryParse(searchString, out var weight) ? query.Where(p => p.Weight == weight) : query.Where(p => p.Weight == -1),
                _ => query,
            };
        }
    }
}
