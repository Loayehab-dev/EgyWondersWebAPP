using EgyWonders.Interfaces;
using EgyWonders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace EgyWonders.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Host")] // CRITICAL: Only Hosts can access this
    public class HostDashboardController : ControllerBase
    {
        private readonly IHostDashbordService _hostService;

        public HostDashboardController(IHostDashbordService hostService)
        {
            _hostService = hostService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            // 1. Get the Host's ID from the JWT Token
            // This claim name ("BusinessId") must match what you saved in AccountController Login
            var userIdClaim = User.FindFirst("BusinessId") ?? User.FindFirst("BusinessUserId");

            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Business ID not found in token." });
            }

            if (!int.TryParse(userIdClaim.Value, out int hostId))
            {
                return BadRequest(new { message = "Invalid Host ID format." });
            }

            // 2. Call the service
            var stats = await _hostService.GetHostStatsAsync(hostId);

            return Ok(stats);
        }
    }
}