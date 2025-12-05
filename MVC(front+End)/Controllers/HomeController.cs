using Microsoft.AspNetCore.Mvc;
using MVC_front_End_.Models;
using MVC_front_End_.Services; // Import Service Namespace
using System.Diagnostics;

namespace MVC_front_End_.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGuestService _guestService; // 1. Inject Service

        public HomeController(ILogger<HomeController> logger, IGuestService guestService)
        {
            _logger = logger;
            _guestService = guestService;
        }

        public async Task<IActionResult> Index()
        {
            // 2. Fetch Data using Service
            var allListings = await _guestService.GetListingsAsync();

            // 3. Take top 6 for Home Page
            var featured = allListings.Take(6).ToList();

            return View(featured);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}