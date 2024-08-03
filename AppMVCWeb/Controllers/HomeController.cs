using App.Models;
using AppMVCWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AppMVCWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public string HiHome()
        {
            return "Hi from Home Controller";
        }
    
        public IActionResult Index()
        {
            var products = _context.Products
                        .Include(p => p.Author)
                        .Include(p => p.Photos)
                        .Include(p => p.ProductCategoryProducts)
                        .ThenInclude(pc => pc.Category)
                        .OrderByDescending(p => p.DateUpdated)
                        .Take(4)
                        .AsQueryable();


            var posts = _context.Posts
                            .Include(p => p.Author)
                            .Include(p => p.PostCategories)
                            .ThenInclude(pc => pc.Category)
                            .OrderByDescending(p => p.DateUpdated)
                            .Take(3)
                            .AsQueryable();

            ViewBag.Products = products;
            ViewBag.Posts = posts;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
