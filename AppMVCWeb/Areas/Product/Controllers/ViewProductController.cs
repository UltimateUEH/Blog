using App.Models;
using App.Models.Product;
using AppMVCWeb.Areas.Product.Models;
using AppMVCWeb.Areas.Product.Services;
using AppMVCWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMVCWeb.Areas.Product.Controllers
{
    [Area("Product")]
    public class ViewProductController : Controller
    {
        private readonly ILogger<ViewProductController> _logger;
        private readonly AppDbContext _context;
        private readonly CartService _cartService;
        private readonly PaypalClient _paypalClient;

        public ViewProductController(ILogger<ViewProductController> logger, AppDbContext context, CartService cartService, PaypalClient paypalClient)
        {
            _logger = logger;
            _context = context;
            _cartService = cartService;
            _paypalClient = paypalClient;
        }

        [Route("/product/{categorySlug?}")]
        public IActionResult Index(string categorySlug, [FromQuery(Name = "p")] int currentPage, int itemPerPage)
        {
            var categoryProducts = GetCategoryProducts();
            ViewBag.categoryProducts = categoryProducts;
            ViewBag.categorySlug = categorySlug;

            CategoryProduct categoryProduct = null;

            if (!string.IsNullOrEmpty(categorySlug))
            {
                categoryProduct = _context.CategoryProducts
                                    .Include(c => c.CategoryChildren)
                                    .FirstOrDefault(c => c.Slug == categorySlug);

                if (categoryProduct == null)
                {
                    return NotFound("Không tìm thấy chuyên mục sản phẩm");
                }

                EnsureAllChildrenLoaded(categoryProduct.CategoryChildren.ToList());
            }

            var products = _context.Products
                                    .Include(p => p.Author)
                                    .Include(p => p.Photos)
                                    .Include(p => p.ProductCategoryProducts)
                                    .ThenInclude(pc => pc.Category)
                                    .OrderByDescending(p => p.DateUpdated)
                                    .AsQueryable();

            if (categoryProduct != null)
            {
                var categoryProductIds = new List<int>();
                categoryProduct.ChildCategoryIds(null, categoryProductIds);
                categoryProductIds.Add(categoryProduct.Id);

                products = products.Where(p => p.ProductCategoryProducts.Any(pc => categoryProductIds.Contains(pc.CategoryId)));
            }

            if (itemPerPage <= 0)
                itemPerPage = 6;

            int totalProducts = products.Count();
            int countPages = (int)Math.Ceiling((double)totalProducts / itemPerPage);

            if (currentPage < 1)
                currentPage = 1;
            if (currentPage > countPages)
                currentPage = countPages;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new { p = pageNumber, itemPerPage = itemPerPage })
            };

            var productsInPage = products.Skip(Math.Max((currentPage - 1) * itemPerPage, 0))
                                .Take(itemPerPage)
                                .ToList();

            ViewBag.PagingModel = pagingModel;
            ViewBag.totalProducts = totalProducts;
            ViewBag.categoryProduct = categoryProduct;

            return View(productsInPage.ToList());
        }

        [Route("/product/{productSlug}.html")]
        public IActionResult Details(string productSlug)
        {
            var categoryProducts = GetCategoryProducts();
            ViewBag.categoryProducts = categoryProducts;

            var product =  _context.Products
                                    .Include(p => p.Author)
                                    .Include(p => p.Photos) 
                                    .Include(p => p.ProductCategoryProducts)
                                    .ThenInclude(pc => pc.Category)
                                    .FirstOrDefault(p => p.Slug == productSlug);

            if (product == null)
            {
                return NotFound("Không tìm thấy sản phẩm");
            }

            ViewData["Title"] = product.Title;

            CategoryProduct categoryProduct = product.ProductCategoryProducts.FirstOrDefault()?.Category;
            ViewBag.categoryProduct = categoryProduct;

            var otherProducts = _context.Products.Where(p => p.ProductCategoryProducts.Any(c => c.Category.Id == categoryProduct.Id))
                                    .Where(p => p.ProductId != product.ProductId)
                                    .OrderByDescending(p => p.DateUpdated)
                                    .Take(5);

            ViewBag.otherProducts = otherProducts;

            return View(product);
        }

        private List<CategoryProduct> GetCategoryProducts()
        {
            var categoryProducts = _context.CategoryProducts
                                    .Include(c => c.CategoryChildren)
                                    .ToList();

            // Filter to only include top-level categories in the final list
            var topLevelCategories = categoryProducts.Where(c => c.ParentCategory == null).ToList();

            return topLevelCategories;
        }

        private void EnsureAllChildrenLoaded(List<CategoryProduct> categoryProducts)
        {
            foreach (var categoryProduct in categoryProducts)
            {
                _context.Entry(categoryProduct).Collection(c => c.CategoryChildren).Load();
                EnsureAllChildrenLoaded(categoryProduct.CategoryChildren.ToList());
            }
        }

        [Route("addcart/{productid:int}", Name = "addcart")]
        public IActionResult AddToCart([FromRoute] int productId)
        {
            try
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
                if (product == null)
                    return NotFound("Không có sản phẩm");

                var cart = _cartService.GetCartItems();
                var cartItem = cart.Find(p => p.Product.ProductId == productId);
                if (cartItem != null)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    cart.Add(new CartItem() { Quantity = 1, Product = product });
                }

                _cartService.SaveCartSession(cart);
                _logger.LogInformation("Product added to cart: {Product}", product);

                return RedirectToAction(nameof(Cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product to cart");
                return StatusCode(500, "Internal server error");
            }
        }

        [Route("/cart", Name = "cart")]
        public IActionResult Cart()
        {
            ViewBag.PaypalClientId = _paypalClient.ClientId;

            var cartItems = _cartService.GetCartItems();
            _logger.LogInformation("Cart items: {CartItems}", cartItems);

            return View(cartItems);
        }

        [Route("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart([FromForm] int productid, [FromForm] int quantity)
        {
            try
            {
                var cart = _cartService.GetCartItems();
                var cartItem = cart.Find(p => p.Product.ProductId == productid);
                if (cartItem != null)
                {
                    cartItem.Quantity = quantity;
                }

                _cartService.SaveCartSession(cart);
                _logger.LogInformation("Updated cart item: {ProductID}, Quantity: {Quantity}", productid, quantity);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return StatusCode(500, "Internal server error");
            }
        }

        [Route("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart([FromRoute] int productid)
        {
            var cart = _cartService.GetCartItems();
            var cartItem = cart.Find(p => p.Product.ProductId == productid);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }

            _cartService.SaveCartSession(cart);
            _logger.LogInformation("Removed cart item: {ProductID}", productid);

            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [Route("/checkout", Name = "checkout")]
        public IActionResult Checkout(string customerName, string customerPhone, string customerAddress)
        {
            var cart = _cartService.GetCartItems();
            if (cart == null || cart.Count == 0)
            {
                return View("Error", new { message = "Giỏ hàng trống" });
            }

            var order = new Order
            {
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                CustomerAddress = customerAddress,
                OrderDate = DateTime.Now,
                OrderStatus = "Đang xử lý",
                TotalAmount = cart.Sum(item => item.Product.Price * item.Quantity),
                PaymentMethod = "COD",
                OrderItems = cart.Select(item => new OrderItem
                {
                    ProductId = item.Product.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            _cartService.ClearCart();

            ViewData["Title"] = "Đặt hàng thành công";
            return View("Success", order);
        }

        [Authorize]
        [Route("/Cart/PaymentSuccess")]
        public IActionResult PaymentSuccess()
        {
            return View("PaypalSuccess");
        }

        #region Paypal payment
        [HttpPost("/Cart/create-paypal-order")]
        [Authorize]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            // Thông tin giỏ hàng qua Paypal
            string totalAmount = _cartService.GetCartItems().Sum(item => item.Product.Price * item.Quantity).ToString();
            string currencyCode = "USD";
            string OrderIdRef = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                var response = await _paypalClient.CreateOrder(totalAmount, currencyCode, OrderIdRef);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var err = new { ex.GetBaseException().Message };
                return BadRequest(err);
            }

        }

        [HttpPost("/Cart/capture-paypal-order")]
        [Authorize]
        public async Task<IActionResult> CapturePaypalOrder(string orderId, [FromBody] CustomerInfo customerInfo, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderId);

                // Lấy thông tin giỏ hàng
                var cart = _cartService.GetCartItems();
                if (cart == null || cart.Count == 0)
                {
                    return View("Error", new { message = "Giỏ hàng trống" });
                }

                // Tạo đối tượng Order
                var order = new Order
                {
                    CustomerName = customerInfo.CustomerName,
                    CustomerPhone = customerInfo.CustomerPhone,
                    CustomerAddress = customerInfo.CustomerAddress,
                    OrderDate = DateTime.Now,
                    OrderStatus = "Đang xử lý",
                    TotalAmount = cart.Sum(item => item.Product.Price * item.Quantity),
                    PaymentMethod = "PayPal",
                    OrderItems = cart.Select(item => new OrderItem
                    {
                        ProductId = item.Product.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    }).ToList()
                };

                // Lưu đối tượng Order vào cơ sở dữ liệu
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(cancellationToken);

                // Xóa giỏ hàng sau khi lưu thành công
                _cartService.ClearCart();

                return Ok(response);
            }
            catch (Exception ex)
            {
                var err = new { ex.GetBaseException().Message };
                return BadRequest(err);
            }
        }

        public class CustomerInfo
        {
            public string CustomerName { get; set; }
            public string CustomerPhone { get; set; }
            public string CustomerAddress { get; set; }
        }

        #endregion
    }
}
