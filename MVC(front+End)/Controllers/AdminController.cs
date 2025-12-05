using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MVC_front_End_.Models;
using MVC_front_End_.Services;
using System.Security.Claims;

namespace MVC_front_End_.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService, IAuthService authService)
        {
            _adminService = adminService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. Get stats from service
            var stats = await _adminService.GetDashboardStatsAsync();

            // 2. Pass to view (Safe check: if null, create empty one)
            return View(stats ?? new DashboardStatsViewModel());
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Listings()
        {
            // Now using the dedicated service!
            var listings = await _adminService.GetAllListingsAsync();
            return View(listings);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // STEP 1: Check Email & Password via API
                var result = await _authService.LoginAsync(model);

                // CHECK 1: Did the API reject the credentials?
                if (result == null )
                {
                    ModelState.AddModelError("", result?.Message ?? "Invalid Email or Password.");
                    return View(model);
                }

               

                // Debugging: Ensure roles list isn't null
                var userRoles = result.Roles ?? new List<string>();

                // Check for "Admin" (Case-Insensitive to be safe)
                bool isAdmin = userRoles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase));

                if (!isAdmin)
                {
                    // The user exists, password is correct, BUT they are just a Guest/Host.
                    ModelState.AddModelError("", "Access Denied: Your account does not have Admin privileges.");
                    return View(model);
                }

               

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, result.Username ?? result.Email),
            new Claim(ClaimTypes.Email, result.Email),
            new Claim("JWT", result.Token ?? ""),
            new Claim(ClaimTypes.NameIdentifier, result.UserId ?? ""),
            // Admins might not have a BusinessId, so default to "0" to prevent crashes
            new Claim("BusinessId", result.BusinessUserId != 0 ? result.BusinessUserId.ToString() : "0")
        };

                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
                    });

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"System Error: {ex.Message}");
                return View(model);
            }
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        [HttpGet]
        public async Task<IActionResult> HostManagement()
        {
            // 1. Get the list of IDs from the API
            var pendingDocs = await _adminService.GetPendingDocumentsAsync();

            // 2. Create the View Model list
            var viewModelList = new List<HostVerificationViewModel>();

            foreach (var doc in pendingDocs)
            {
                // Optional: Fetch user name from API if you have an endpoint for it
                // var user = await _adminService.GetUserById(doc.UserId); 

                viewModelList.Add(new HostVerificationViewModel
                {
                    DocumentId = doc.DocumentId,
                    UserId = doc.UserId,
                    NationalId = doc.NationalId,
                    DocumentPath = doc.DocumentPath,
                    CreatedAt = doc.CreatedAt,
                    // If you can't fetch the user, you can just label it nicely:
                    ApplicantName = $"Applicant #{doc.UserId}"
                });
            }

            return View(viewModelList);
        }
        [HttpPost]
        public async Task<IActionResult> ApproveHost(int id)
        {
            // 'id' represents the DocumentId (HostDocument.DocumentId) passed from the form
            var success = await _adminService.ApproveDocumentAsync(id);

            if (success)
            {
                TempData["success"] = $"Host Document ID {id} approved successfully! User is now a Host.";
            }
            else
            {
                // This will trigger if the API returns 404/400 (e.g., document not found, bad data)
                TempData["error"] = $"Failed to approve Host Document ID {id}. Document may not exist or user data is invalid.";
            }

            // Redirect back to the HostManagement page or Dashboard (Index)
            // Redirecting to HostManagement is usually better contextually if you have a list there.
            return RedirectToAction("HostManagement");
        }
       

        [HttpPost]
        public async Task<IActionResult> RejectHost(int id)
        {
            var success = await _adminService.RejectDocumentAsync(id);

            if (success)
            {
                TempData["success"] = "Request rejected and document deleted successfully.";
            }
            else
            {
                TempData["error"] = "Failed to reject request. It may have already been deleted.";
            }

            return RedirectToAction("HostManagement");
        }
        // 1. VIEW DETAILS PAGE
        [HttpGet]
        public async Task<IActionResult> ListingDetails(int id)
        {
            var listing = await _adminService.GetListingByIdAsync(id);
            if (listing == null) return NotFound();
            return View(listing);
        }

        // 2. Change Status (Approve)
        [HttpPost]
        public async Task<IActionResult> ChangeListingStatus(int id, string status)
        {
            var success = await _adminService.UpdateListingStatusAsync(id, status);
            if (success) TempData["success"] = "Status updated successfully!";
            else TempData["error"] = "Failed to update status.";

            return RedirectToAction("Listings");
        }

        // 3. Delete Listing
        [HttpPost]
        public async Task<IActionResult> DeleteListing(int id)
        {
            var success = await _adminService.DeleteListingAsync(id);
            if (success) TempData["success"] = "Listing deleted.";
            else TempData["error"] = "Failed to delete listing.";

            return RedirectToAction("Listings");
        }
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            // Fetch users from the service
            var users = await _adminService.GetAllUsersAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var success = await _adminService.DeleteUserAsync(id);

            if (success)
            {
                TempData["success"] = "User deleted successfully.";
            }
            else
            {
                TempData["error"] = "Failed to delete user.";
            }

            return RedirectToAction(nameof(Users));
        }
              [  HttpGet]
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _adminService.GetAllBookingsAsync();
            return View(bookings);
        }
        [HttpGet]
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _adminService.GetAllReviewsAsync();
            return View(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var success = await _adminService.DeleteReviewAsync(id);

            if (success) TempData["success"] = "Review deleted successfully.";
            else TempData["error"] = "Failed to delete review.";

            return RedirectToAction(nameof(Reviews));
        }
    }
}