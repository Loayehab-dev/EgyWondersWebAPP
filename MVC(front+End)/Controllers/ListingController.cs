using Microsoft.AspNetCore.Mvc;
using MVC_front_End_.Services;
using System.Threading.Tasks;

namespace MVC_front_End_.Controllers
{
    public class ListingController : Controller
    {
        private readonly IGuestService _guestService;

        public ListingController(IGuestService guestService)
        {
            _guestService = guestService;
        }

       
        
        public async Task<IActionResult> Index(string search, string category, decimal? minPrice, decimal? maxPrice, int? capacity)
        {
            // Pass filters to service
            var listings = await _guestService.GetListingsAsync(search, category, minPrice, maxPrice, capacity);

            // Keep filters in ViewBag so the UI inputs stay filled
            ViewBag.Search = search;
            ViewBag.Category = category;

            return View(listings);
        }

        // 2. DETAILS (Single Page)
        [HttpGet("Listing/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var listing = await _guestService.GetListingByIdAsync(id);

            if (listing == null) return NotFound();

            return View(listing);
        }

    }
}