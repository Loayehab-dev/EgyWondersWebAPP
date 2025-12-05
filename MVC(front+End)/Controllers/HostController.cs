using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_front_End_.Models;
using MVC_front_End_.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;


namespace MVC_front_End_.Controllers
{
    [Authorize] 
    public class HostController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IHostService _hostService;

        // 2. Inject both services
        public HostController(IAuthService authService, IHostService hostService)
        {
            _authService = authService;
            _hostService = hostService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var stats = await _hostService.GetDashboardStatsAsync();
                return View(stats);
            }
            catch
            {
                return View(new HostDashboardViewModel());
            }
        }


        // Inside MVC_front_End_.Controllers.HostController

        [HttpGet]
        public async Task<IActionResult> MyListings()
        {
            // 1. Get ID from Standard Cookie Claim
            var claim = User.FindFirst("BusinessId");

            // 2. If missing, force logout to refresh the cookie
            if (claim == null || string.IsNullOrEmpty(claim.Value) || claim.Value == "0")
            {
                return RedirectToAction("Logout", "Auth");
            }

            int hostId = int.Parse(claim.Value);

            try
            {
                // 3. Call Service
                var listings = await _hostService.GetHostListingsByIdAsync(hostId);
                return View(listings);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            // If already a host, kick them back to dashboard
            if (User.IsInRole("Host"))
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(HostDocumentCreateDTO model)
        {
            // A. Get Integer ID
            var businessIdClaim = User.FindFirst("BusinessId");

            if (businessIdClaim == null)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login", "Auth");
            }

            model.UserId = int.Parse(businessIdClaim.Value);

            // B. Validate File
            if (model.Document == null || model.Document.Length == 0)
            {
                ModelState.AddModelError("Document", "Please upload a valid document.");
                return View(model);
            }

            // C. Call API
            bool isSuccess = await _authService.RegisterHostAsync(model);

            if (isSuccess)
            {
                // --- CLAIM UPDATE LOGIC (Hot Swap) ---
                var currentClaims = new List<Claim>(User.Claims);

                if (!currentClaims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Host"))
                {
                    currentClaims.Add(new Claim(ClaimTypes.Role, "Host"));
                }

                var newIdentity = new ClaimsIdentity(currentClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                var newPrincipal = new ClaimsPrincipal(newIdentity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    newPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = System.DateTime.UtcNow.AddDays(14)
                    }
                );

                TempData["SuccessMessage"] = "Application Submitted! Welcome to your Dashboard.";

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Failed to upload document.");
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> CreateListing()
        {
            // 1. Fetch from DB
            var amenities = await _hostService.GetAmenitiesAsync();

            // 2. Pass to View via ViewBag
            ViewBag.Amenities = amenities;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateListing(ListingCreateDTO model)
        {
            // 1. Check Validation
            if (!ModelState.IsValid)
            {
                // RELOAD amenities so checkboxes don't disappear on error
                ViewBag.Amenities = await _hostService.GetAmenitiesAsync();
                return View(model);
            }

            // 2. Call Service
            var success = await _hostService.CreateListingAsync(model);

            if (success)
            {
                TempData["SuccessMessage"] = "Listing created successfully!";
                return RedirectToAction("Index");
            }

            // 3. Handle Failure
            ModelState.AddModelError("", "Failed to create listing.");
            ViewBag.Amenities = await _hostService.GetAmenitiesAsync();
            return View(model);
        }
        // GET: Edit Page
        [HttpGet]
        public async Task<IActionResult> EditListing(int id)
        {
            // 1. Fetch existing data
            var listing = await _hostService.GetListingByIdAsync(id);
            if (listing == null) return NotFound();

            // 2. Fetch all available amenities (for checkboxes)
            ViewBag.AllAmenities = await _hostService.GetAmenitiesAsync();

            // 3. Map to UpdateDTO
            var model = new ListingUpdateDTO
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                // Description = listing.Description, // Make sure ListingDTO has this
                PricePerNight = listing.PricePerNight,
                Capacity = listing.Capacity,
                CityName = listing.CityName,
                Category = listing.Category,
                // Map existing photos to display them
                ExistingPhotos = listing.Photos ?? new List<ListingPhotoDTO>()
            };

            return View(model);
        }

        // POST: Save Changes
       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditListing(int id, ListingUpdateDTO model)
        {
            // 1. Check for Validation Errors (e.g. missing title, price format)
            if (!ModelState.IsValid)
            {
                // Collect errors to show in Toastr
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = "Validation Failed: " + string.Join(", ", errors);

                // Reload data so the view doesn't break
                ViewBag.AllAmenities = await _hostService.GetAmenitiesAsync();
                var existing = await _hostService.GetListingByIdAsync(id);
                if (existing != null) model.ExistingPhotos = existing.Photos;

                return View(model);
            }

            // 2. Attempt Update via Service
            var success = await _hostService.UpdateListingAsync(id, model);

            if (success)
            {
                TempData["SuccessMessage"] = "Listing updated successfully!";
                return RedirectToAction("Index");
            }

            // 3. Handle API Failure
            TempData["ErrorMessage"] = "Update Failed. The server could not save your changes.";

            // Reload data before returning view
            ViewBag.AllAmenities = await _hostService.GetAmenitiesAsync();
            var current = await _hostService.GetListingByIdAsync(id);
            if (current != null) model.ExistingPhotos = current.Photos;

            return View(model);
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteListing(int id)
        {
            var success = await _hostService.DeleteListingAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = "Listing deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not delete listing. It may have active bookings.";
            }

            return RedirectToAction("Index");
        }
       
       
        
        [HttpGet]
        public async Task<IActionResult> Bookings()
        {
            // Reuse the stats service to get the bookings list
            var stats = await _hostService.GetDashboardStatsAsync();

            // Pass the list of bookings to the view
            return View(stats.RecentBookings);
        }

        [HttpGet]
        public async Task<IActionResult> Earnings()
        {
            // Reuse the stats service to get earnings totals
            var stats = await _hostService.GetDashboardStatsAsync();

            // Pass the whole model so we can show TotalEarnings and Bookings breakdown
            return View(stats);
        }

    }
}