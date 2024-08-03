using App.Data;
using App.Models;
using AppMVCWeb.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppMVCWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator)]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [Route("/admin/dashboard")]
        public IActionResult Index()
        {
            var revenueData = _context.Orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new RevenueViewModel
                {
                    MonthYear = $"{g.Key.Month}/{g.Key.Year}",
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .ToList();

            ViewBag.RevenueData = Newtonsoft.Json.JsonConvert.SerializeObject(revenueData);
            ViewBag.TotalRevenue = revenueData.Sum(r => r.TotalRevenue);

            return View();
        }
    }
}
