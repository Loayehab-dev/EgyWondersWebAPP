using EgyWonders.DTO;
using EgyWonders.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static EgyWonders.DTO.RegisterDTO;

namespace EgyWonders.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AuthController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [EnableRateLimiting
            ("Fixed")]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            try { return Ok(await _accountService.RegisterAsync(dto)); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }
        [EnableRateLimiting("Fixed")]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            try { return Ok(await _accountService.LoginAsync(dto)); }
            catch (Exception ex) { return Unauthorized(new { error = ex.Message }); }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            try { return Ok(await _accountService.RefreshTokenAsync(dto.Token)); }
            catch (Exception ex) { return Unauthorized(new { error = ex.Message }); }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _accountService.ChangePasswordAsync(userId, dto);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(await _accountService.GetProfileAsync(userId));
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDTO dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                await _accountService.UpdateProfileAsync(userId, dto);
                return Ok(new { message = "Profile updated successfully" });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        // DELETE: api/auth/account
        [HttpDelete("account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDTO model)
        {
            // 1. Validation Check
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 2. Safety Check: Did they check the box?
            if (!model.ConfirmDeletion)
            {
                return BadRequest(new { message = "You must confirm deletion by setting ConfirmDeletion to true." });
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // 3. Call Service (Passes password for verification)
                await _accountService.DeleteAccountAsync(userId, model.Password);

                return Ok(new { message = "Account deleted successfully" });
            }
            catch (Exception ex)
            {
                // Handles invalid password errors
                return BadRequest(new { error = ex.Message });
            }
        }

        // --- ADMIN ACTIONS ---

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(AssignRoleDTO dto)
        {
            try
            {
                await _accountService.AssignRoleAsync(dto);
                return Ok(new { message = $"Role {dto.RoleName} assigned." });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPost("remove-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole(AssignRoleDTO dto)
        {
            try
            {
                await _accountService.RemoveRoleAsync(dto);
                return Ok(new { message = $"Role {dto.RoleName} removed." });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPatch("users/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id, [FromQuery] bool isActive)
        {
            try
            {
                await _accountService.ToggleUserStatusAsync(id, isActive);
                return Ok(new { message = $"User status updated to {(isActive ? "Active" : "Inactive")}" });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _accountService.GetAllUsersAsync());
        }

        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _accountService.GetUserByIdAsync(id);
            if (user == null) return NotFound("User not found");
            return Ok(user);
        }
        [HttpGet("debug-user")]
        public IActionResult DebugUser()
        {
            var user = HttpContext.User;

            return Ok(new
            {
                IsAuthenticated = user.Identity?.IsAuthenticated,
                AuthenticationType = user.Identity?.AuthenticationType,
                Claims = user.Claims.Select(c => new { c.Type, c.Value })
            });
        }
        [EnableRateLimiting("Fixed")]
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            try
            {
                var result = await _accountService.ConfirmEmailAsync(userId, token);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [EnableRateLimiting("Fixed")]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO dto)
        {
            try
            {
                var result = await _accountService.ForgotPasswordAsync(dto.Email);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [EnableRateLimiting("Fixed")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
        {
            try
            {
                var result = await _accountService.ResetPasswordAsync(dto);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [EnableRateLimiting("Fixed")]
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO dto)
        {
            try
            {
                var result = await _accountService.GoogleLoginAsync(dto.IdToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}