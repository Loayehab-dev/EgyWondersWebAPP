using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using MVC_front_End_.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using MVC_front_End_.Models;

namespace MVC_front_End_.Controllers
{
    [Authorize] 
    public class GuestController : Controller
    {
        private readonly GuestBookingService _bookingService;

        public GuestController(GuestBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task<IActionResult> Trips()
        {
            // 1. Get the Business ID (Integer) instead of NameIdentifier
            var businessIdClaim = User.FindFirst("BusinessId");

            // Security check
            if (businessIdClaim == null || businessIdClaim.Value == "0")
            {
                return RedirectToAction("Login", "Auth");
            }

            // 2. Parse the Integer
            int userId = int.Parse(businessIdClaim.Value);

            var bookings = await _bookingService.GetUserBookingsAsync(userId);

            return View(bookings);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int listingId, int rating, string comment)
        {
            // 1. Try to find the ID in the specific "UserId" claim first
            //    (If that fails, try "BusinessId", then fall back to standard NameIdentifier)
            var claim = User.FindFirst("UserId")
                     ?? User.FindFirst("BusinessId")
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            // 2. If no user is found, redirect to login
            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 3. SAFELY parse the ID (This fixes the crash)
            if (!int.TryParse(claim.Value, out int userId))
            {
                // If we are here, it means the ID is a GUID (String) but we need an Int.
                // Log out the user to fix the bad cookie.
                TempData["Error"] = "Session invalid. Please login again.";
                return RedirectToAction("Logout", "Auth");
            }

            // 4. Create the DTO
            var review = new ReviewCreateDTO
            {
                ListingId = listingId,
                UserId = userId,
                Rating = rating,
                Comment = comment
            };

            // 5. Send to Service
            var success = await _bookingService.AddReviewAsync(review);

            if (success)
            {
                TempData["SuccessMessage"] = "Review submitted! Thank you.";
            }
            else
            {
                TempData["Error"] = "Failed to submit review.";
            }

            return RedirectToAction("Trips");
        }
    }
}