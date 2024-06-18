using Microsoft.AspNetCore.Mvc;
using RPBDlab3_Netkachev_Ustinov.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NPOI.XWPF.UserModel;
using System.Data;
using Microsoft.AspNetCore.SignalR;

namespace RPBDlab3_Netkachev_Ustinov.Controllers
{
    public class OrdersController : Controller
    {
        private readonly DatabaseContext _context;
        private static List<Orders> lastView = new List<Orders>();
        private readonly IHubContext<ObserverUpdater> _hubContext;

        public OrdersController(DatabaseContext context, IHubContext<ObserverUpdater> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // SORT
        private IQueryable<Orders> ApplySorting(IQueryable<Orders> ordersQuery, string sortOrder)
        {
            return sortOrder switch
            {
                "customer_desc" => ordersQuery.OrderByDescending(o => o.Customer.CustomerName),
                "Product" => ordersQuery.OrderBy(o => o.Product.ProductName),
                "product_desc" => ordersQuery.OrderByDescending(o => o.Product.ProductName),
                "OrderDate" => ordersQuery.OrderBy(o => o.OrderDate),
                "orderDate_desc" => ordersQuery.OrderByDescending(o => o.OrderDate),
                "OrderStatus" => ordersQuery.OrderBy(o => o.OrderStatus.StatusString),
                "orderStatus_desc" => ordersQuery.OrderByDescending(o => o.OrderStatus.StatusString),
                "PickupPoint" => ordersQuery.OrderBy(o => o.PickupPoint.PickupPointAddress),
                "pickupPoint_desc" => ordersQuery.OrderByDescending(o => o.PickupPoint.PickupPointAddress),
                "Amount" => ordersQuery.OrderBy(o => o.AmountOfProducts),
                "amount_desc" => ordersQuery.OrderByDescending(o => o.AmountOfProducts),
                "DeliveryDate" => ordersQuery.OrderBy(o => o.DeliveryDate),
                "deliveryDate_desc" => ordersQuery.OrderByDescending(o => o.DeliveryDate),
                _ => ordersQuery.OrderBy(o => o.Customer.CustomerName),
            };
        }

        // SEARCH
        private IQueryable<Orders> ApplySearch(IQueryable<Orders> ordersQuery, string searchString, string searchColumn)
        {
            return searchColumn switch
            {
                "name" => ordersQuery.Where(s => s.Customer.CustomerName.ToLower().Contains(searchString.ToLower())),
                "productname" => ordersQuery.Where(s => s.Product.ProductName.ToLower().Contains(searchString.ToLower())),
                "orderdate" => ordersQuery.Where(s => s.OrderDate.ToString().ToLower().Contains(searchString.ToLower())),
                "orderstatus" => ordersQuery.Where(s => s.OrderStatus.StatusString.ToLower().Contains(searchString.ToLower())),
                "address" => ordersQuery.Where(s => s.PickupPoint.PickupPointAddress.ToLower().Contains(searchString.ToLower())),
                "pts" when Int64.TryParse(searchString, out _) => ordersQuery.Where(s => s.AmountOfProducts == Convert.ToInt64(searchString)),
                "pts" => ordersQuery.Where(s => s.AmountOfProducts == -1),
                "deliverydate" => ordersQuery.Where(s => s.DeliveryDate.ToString().ToLower().Contains(searchString.ToLower())),
                _ => ordersQuery,
            };
        }

        private async Task<List<Orders>> GetFilteredAndSortedOrders(string searchString, string searchColumn, string sortOrder)
        {
            IQueryable<Orders> ordersQuery = _context.Orders.Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .Include(o => o.PickupPoint)
                .Include(o => o.Product);

            ordersQuery = ApplySorting(ordersQuery, sortOrder);

            if (!String.IsNullOrEmpty(searchString))
            {
                ordersQuery = ApplySearch(ordersQuery, searchString, searchColumn);
            }

            return await ordersQuery.ToListAsync();
        }
        public async Task<IActionResult> Index(string searchString, string searchColumn, string sortOrder)
        {
            ViewData["CustomerSortParam"] = String.IsNullOrEmpty(sortOrder) ? "customer_desc" : "";
            ViewData["ProductSortParam"] = sortOrder == "Product" ? "product_desc" : "Product";

            var filteredAndSortedOrders = await GetFilteredAndSortedOrders(searchString, searchColumn, sortOrder);

            lastView = filteredAndSortedOrders;
            return View(filteredAndSortedOrders);
        }

        // GET: Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerId,ProductId,PickupPointId,OrderStatusId,AmountOfProducts,OrderDate,DeliveryDate")] Orders orders)
        {
            if (ModelState.IsValid)
            {
                if (await TryUpdateProductAmountAsync(orders.ProductId, orders.AmountOfProducts))
                {
                    _context.Add(orders);
                    await _context.SaveChangesAsync();
                    await NotifyClientsAsync(orders.Id);
                    return RedirectToAction(nameof(Index));
                }
            }

            PopulateDropdowns();
            return View(orders);
        }


        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }

            PopulateDropdowns();
            return View(orders);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,ProductId,PickupPointId,OrderStatusId,AmountOfProducts,OrderDate,DeliveryDate")] Orders orders)
        {
            if (id != orders.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await TryUpdateProductAmountAsync(orders.ProductId, orders.AmountOfProducts))
                {
                    _context.Update(orders);
                    await _context.SaveChangesAsync();
                    await NotifyClientsAsync(orders.Id);
                    return RedirectToAction(nameof(Index));
                }
            }

            PopulateDropdowns();
            return View(orders);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .Include(o => o.PickupPoint)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orders = await _context.Orders.FindAsync(id);

            if (orders != null)
            {
                _context.Orders.Remove(orders);
                await _context.SaveChangesAsync();
                await NotifyClientsAsync(orders.Id);
            }

            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "CustomerName");
            ViewData["OrderStatusId"] = new SelectList(_context.OrderStatuses, "Id", "StatusString");
            ViewData["PickupPointId"] = new SelectList(_context.PickupPoints, "Id", "PickupPointAddress");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "ProductName");
        }

        private async Task<bool> TryUpdateProductAmountAsync(int productId, int requestedAmount)
        {
            var selectedProduct = await _context.Products.FindAsync(productId);

            if (selectedProduct != null)
            {
                int availableAmount = selectedProduct.Amount;

                if (availableAmount >= requestedAmount)
                {
                    selectedProduct.Amount -= requestedAmount;
                    return true;
                }

                ModelState.AddModelError("AmountOfProducts", $"Столько товара нет. Доступно: {availableAmount}");
            }
            else
            {
                ModelState.AddModelError("ProductId", "Ошибка в выборе товара");
            }

            return false;
        }

        private async Task NotifyClientsAsync(int orderId)
        {
            await _hubContext.Clients.All.SendAsync("SendUpdateNotification", orderId);
        }

        private bool OrdersExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
        public IActionResult ExportToExcel(string sortOrder, string searchString, string searchColumn)
        {


            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Orders");
                var headers = new List<string> { "Покупатель", "Товар", "Дата заказа", "Статус заказа", "Пункт выдачи", "Количество", "Дата доставки" };

                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                }

                for (int i = 0; i < lastView.Count; i++)
                {
                    var order = lastView[i];
                    worksheet.Cell(i + 2, 1).Value = order.Customer.CustomerName;
                    worksheet.Cell(i + 2, 2).Value = order.Product.ProductName;
                    worksheet.Cell(i + 2, 3).Value = order.OrderDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(i + 2, 4).Value = order.OrderStatus.StatusString;
                    worksheet.Cell(i + 2, 5).Value = order.PickupPoint.PickupPointAddress;
                    worksheet.Cell(i + 2, 6).Value = order.AmountOfProducts;
                    worksheet.Cell(i + 2, 7).Value = order.DeliveryDate.ToString("yyyy-MM-dd");
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Заказы.xlsx");
                }
            }
        }

        public IActionResult ExportToWord(string sortOrder, string searchString, string searchColumn)
        {
            var doc = new XWPFDocument();
            var table = doc.CreateTable(lastView.Count + 1, 7);

            var headers = new List<string> { "Покупатель", "Товар", "Дата заказа", "Статус заказа", "Пункт выдачи", "Количество", "Дата доставки" };

            var headerRow = table.GetRow(0);
            for (int i = 0; i < headers.Count; i++)
            {
                var cell = headerRow.GetCell(i);
                cell.SetText(headers[i]);
            }

            for (int i = 0; i < lastView.Count; i++)
            {
                var order = lastView[i];
                var row = table.GetRow(i + 1);

                SetCellValue(row.GetCell(0), order.Customer.CustomerName);
                SetCellValue(row.GetCell(1), order.Product.ProductName);
                SetCellValue(row.GetCell(2), order.OrderDate.ToString("yyyy-MM-dd"));
                SetCellValue(row.GetCell(3), order.OrderStatus.StatusString);
                SetCellValue(row.GetCell(4), order.PickupPoint.PickupPointAddress);
                SetCellValue(row.GetCell(5), order.AmountOfProducts.ToString());
                SetCellValue(row.GetCell(6), order.DeliveryDate.ToString("yyyy-MM-dd"));
            }

            using (var stream = new MemoryStream())
            {
                doc.Write(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Заказы.docx");
            }
        }

        private void SetCellValue(XWPFTableCell cell, string value)
        {
            cell.SetText(value);
        }

    }
}
