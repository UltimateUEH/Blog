using App.Models;
using AppMVCWeb.Areas.Product.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMVCWeb.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("admin/orders/category/[action]/{id?}")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách đơn hàng
        public async Task<IActionResult> Index(int page = 1)
        {
            var itemPerPage = 10; // Số mục trên mỗi trang

            // Đảm bảo currentPage không nhỏ hơn 1
            if (page < 1)
            {
                page = 1;
            }

            var ordersQuery = _context.Orders.AsQueryable();
            var totalOrders = await ordersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)itemPerPage);

            // Đảm bảo currentPage không vượt quá tổng số trang
            if (page > totalPages)
            {
                page = totalPages;
            }

            // Nếu không có đơn hàng nào, trả về view với danh sách trống
            if (totalOrders == 0)
            {
                ViewBag.PagingModel = new PagingModel
                {
                    currentpage = page,
                    countpages = 1, // Có một trang giả để hiển thị phân trang
                    generateUrl = p => Url.Action("Index", new { page = p })
                };
                return View(new List<Order>());
            }

            var orders = await ordersQuery
                .OrderBy(o => o.OrderDate) // Sắp xếp theo ngày đơn hàng hoặc theo yêu cầu
                .Skip((page - 1) * itemPerPage)
                .Take(itemPerPage)
                .ToListAsync();

            // Tạo đối tượng PagingModel để sử dụng trong view
            ViewBag.PagingModel = new PagingModel
            {
                currentpage = page,
                countpages = totalPages,
                generateUrl = p => Url.Action("Index", new { page = p })
            };

            return View(orders);
        }

        // Hiển thị chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                                      .Include(o => o.OrderItems)
                                      .ThenInclude(oi => oi.Product)
                                      .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: /Product/Order/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Xóa các OrderItems liên quan
            var orderItems = _context.OrderItems.Where(oi => oi.OrderId == id).ToList();
            _context.OrderItems.RemoveRange(orderItems);

            // Xóa Order
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Xác nhận đơn hàng là đã hoàn thành
        public async Task<IActionResult> MarkAsCompleted(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = "Hoàn thành";
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Create a new order (for demonstration purposes)
        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            if (ModelState.IsValid)
            {
                order.OrderDate = DateTime.Now;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }
    }
}
