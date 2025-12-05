using MVC_front_End_.Models; // Ensure this matches your DTO namespace
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MVC_front_End_.Services;

namespace MVC_front_End_.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

      
        // 1. LOGIN & LOGOUT
      

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.LoginAsync(model);

            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                // ... (Claims creation logic remains the same) ...
                var claims = new List<Claim>
{
                  new Claim(ClaimTypes.Name, result.Username ?? result.Email),
                   new Claim(ClaimTypes.Email, result.Email),
                     new Claim("JWT", result.Token),
                     new Claim("BusinessId", result.BusinessUserId.ToString()),


    new Claim(ClaimTypes.NameIdentifier, result.UserId ?? "")
};

                if (result.Roles != null)
                {
                    foreach (var role in result.Roles)
                    {
                      
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

              
                var authProperties = new AuthenticationProperties
                {
                    // If RememberMe is true, the cookie stays after browser close.
                    // If false, it is a "Session Cookie" and deletes when browser closes.
                    IsPersistent = model.RememberMe,

                    // Set expiration: 14 days if RememberMe, otherwise 60 mins
                    ExpiresUtc = model.RememberMe
                        ? DateTime.UtcNow.AddDays(14)
                        : DateTime.UtcNow.AddMinutes(60)
                };
               

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", result?.Message ?? "Invalid login attempt.");
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

       
        // 2. REGISTRATION
        

        [HttpGet]
        public IActionResult Register() => View();


        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            // Default to Guest if not set
            if (string.IsNullOrEmpty(model.Role)) model.Role = "Guest";

            var result = await _authService.RegisterAsync(model);

            // SMART CHECK: Returns true if authenticated OR if the message implies email verification
            bool isSuccess = result != null && (
                result.IsAuthenticated ||
                (!string.IsNullOrEmpty(result.Message) &&
                 (result.Message.ToLower().Contains("sent") ||
                  result.Message.ToLower().Contains("verify") ||
                  result.Message.ToLower().Contains("confirm")))
            );

            if (isSuccess)
            {
                // UNIFIED SUCCESS HANDLING
                // We force the message into TempData so the Login View can display it nicely.
                // We ignore the specific 'result.Message' text and write our own clear instructions.
                TempData["SuccessMessage"] = "Registration successful! Please check your email to confirm your account before logging in.";

                return RedirectToAction("Login", "Auth");
            }

            // --- FAILURE HANDLING ---
            // If we get here, it means it wasn't authenticated AND didn't contain success keywords.
            ModelState.AddModelError("", result?.Message ?? "Registration failed.");
            return View(model);
        }
        // 3. PASSWORD MANAGEMENT (Forgot / Reset)


        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            await _authService.ForgotPasswordAsync(model.Email);

            // Always show success message for security (prevent email enumeration)
            ViewBag.Message = "If that email exists, a reset link has been sent.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            // Validate link parameters
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordDTO { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var resultString = await _authService.ResetPasswordAsync(model);

            // Match the specific success string from your AuthService
            if (resultString == "Password Reset")
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            ModelState.AddModelError("", "Failed to reset password. Link may be expired.");
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation() => View();

        // 4. AUTHENTICATED USER ACTIONS (Profile / Change Pass)

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            // ★ FIX: Get the token from the User Claims (where Login saved it)
            // NOT from Request.Cookies
            var token = User.FindFirst("JWT")?.Value;

            if (string.IsNullOrEmpty(token))
            {
                // If the token isn't in the claim, the user isn't properly logged in
                return RedirectToAction("Login");
            }

            // Call the service with the valid token
            var userProfile = await _authService.GetUserProfileAsync(token);

            return View(userProfile);
        }
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword() => View();

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = User.FindFirst("JWT_Token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            // Call service (returns "Success" or error message string)
            var result = await _authService.ChangePasswordAsync(model, token);

            if (result == "Success")
            {
                ViewBag.Message = "Your password has been changed successfully!";
                ModelState.Clear();
                return View(new ChangePasswordDTO());
            }

            ModelState.AddModelError("", result);
            return View(model);
        }

       
        // 5. ADMIN ACTIONS
        

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UsersList()
        {
            var token = User.FindFirst("JWT_Token")?.Value;
            var users = await _authService.GetAllUsersAsync(token);
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id, bool status)
        {
            var token = User.FindFirst("JWT_Token")?.Value;
            await _authService.ToggleUserStatusAsync(id, status, token);
            return RedirectToAction("UsersList");
        }
        [HttpPost]
        public async Task<IActionResult> GoogleLogin(string idToken)
        {
            if (string.IsNullOrEmpty(idToken))
            {
                return RedirectToAction("Login");
            }

            // 1. Call Service to exchange Google Token for our App Token
            var result = await _authService.GoogleLoginAsync(idToken);

            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                // 2. Create Claims (Same logic as normal Login)
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, result.Username ?? result.Email),
            new Claim(ClaimTypes.Email, result.Email),
            new Claim("JWT_Token", result.Token)
        };

                if (result.Roles != null)
                {
                    foreach (var role in result.Roles) claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // 3. Create Cookie
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties { IsPersistent = true });

                return RedirectToAction("Index", "Home");
            }

            // Handle failure
            TempData["ErrorMessage"] = "Google Login failed.";
            return RedirectToAction("Login");
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _authService.ConfirmEmailAsync(userId, token);

            if (result == "Success")
            {
                return View("ConfirmEmailSuccess");
            }

            ViewBag.Error = result;
            return View("ConfirmEmailFailure");
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken] // Always required for POST forms
        public async Task<IActionResult> Profile(UpdateProfileDTO model)
        {
            // 1. Basic Validation
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check the highlighted fields.";
                // Fallback: Redirect to GET to show the updated (or current) view
                return RedirectToAction("Profile");
            }

            // 2. Retrieve Token
            var token = User.FindFirst("JWT")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            // 3. Call Service
            var success = await _authService.UpdateUserProfileAsync(model, token);

            // 4. Handle Result
            if (success)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update profile. Please ensure data is valid.";
            }

            // Redirect to the GET Profile action to reload the data and show the message
            return RedirectToAction("Profile");
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var token = User.FindFirst("JWT")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            // Fetch current profile data to pre-fill the inputs
            var currentProfile = await _authService.GetUserProfileAsync(token);

            // Map to DTO
            var model = new UpdateProfileDTO
            {
                FirstName = currentProfile.FirstName,
                LastName = currentProfile.LastName,
                Phone = currentProfile.Phone,
                Nationality = currentProfile.Nationality,
                Gender = currentProfile.Gender,
                DateOfBirth = currentProfile.DateOfBirth
            };

            return View(model);
        }

        // 2. POST: Submit Changes
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UpdateProfileDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = User.FindFirst("JWT")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            // Call API
            var success = await _authService.UpdateUserProfileAsync(model, token);

            if (success)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile"); // Go back to Read-Only View
            }

            ModelState.AddModelError("", "Failed to update profile.");
            return View(model);
        }

    }
}