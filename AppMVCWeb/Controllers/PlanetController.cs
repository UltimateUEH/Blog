using AppMVCWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppMVCWeb.Controllers
{
    [Route("he-mat-troi/[action]")]
    public class PlanetController : Controller
    {
        private readonly PlanetService _planetService;

        private readonly ILogger<PlanetController> _logger;

        public PlanetController(PlanetService plantService, ILogger<PlanetController> logger)
        {
            _planetService = plantService;
            _logger = logger;
        }

        [Route("/danh-sach-cac-hanh-tinh.html")]
        public ActionResult Index() // Route: /he-mat-troi/danh-sach-cac-hanh-tinh.html
        {
            _logger.LogInformation("Index action is called");

            return View();
        }

        // Route: Action
        [BindProperty(SupportsGet = true, Name = "action")]
        public string Name { get; set; } // Action ~ Planet Model

        public IActionResult Mercury()
        {
            var planet = _planetService.FirstOrDefault(p => p.Name == Name);

            return View("Detail", planet);
        }

        public IActionResult Venus()
        {
            var planet = _planetService.FirstOrDefault(p => p.Name == Name);

            return View("Detail", planet);
        }

        public IActionResult Earth()
        {
            var planet = _planetService.FirstOrDefault(p => p.Name == Name);

            return View("Detail", planet);
        }

        public IActionResult Mars()
        {
            var planet = _planetService.FirstOrDefault(p => p.Name == Name);

            return View("Detail", planet);
        }

        [HttpGet("/saomoc.html")]
        public IActionResult Jupiter()
        {
            var planet = _planetService.FirstOrDefault(p => p.Name == Name);

            return View("Detail", planet);
        }

        public IActionResult Saturn()
        {
            var planet = _planetService.FirstOrDefault(p => p.Name == Name);

            return View("Detail", planet);
        }

        public IActionResult Uranus()
        {
            var planet = _planetService.FirstOrDefault(p => p.Name == Name);

            return View("Detail", planet);
        }

        [Route("sao/[action]", Name = "neptune3" ,Order = 3)]                         // sao/Neptune
        [Route("sao/[controller]/[action]", Name = "neptune2", Order = 2)]            // sao/planet/Neptune
        [Route("[controller]-[action].html", Name = "neptune1" , Order = 1)]           // * planet-Neptune.html
        public IActionResult Neptune()
        {
            var planet = _planetService.FirstOrDefault(p => p.Name == Name);

            return View("Detail", planet);
        }

        // controller, action, area => [controller] [action] [area]

        [Route("thong-tin-hanh-tinh/{id:int}")] // Route: /thong-tin-hanh-tinh/1
        public IActionResult PlanetInfo(int id)
        {
            var planet = _planetService.FirstOrDefault(p => p.Id == id);

            return View("Detail", planet);
        }
    }
}
