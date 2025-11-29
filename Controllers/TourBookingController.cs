using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EgyWonders.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires login for all actions
    public class TourBookingsController : ControllerBase
    {
        private readonly ITourBookingService _tourBookingService;

        public TourBookingsController(ITourBookingService tourBookingService)
        {
            _tourBookingService = tourBookingService;
        }

        // POST: api/TourBookings
        [HttpPost]
        public async Task<IActionResult> BookTour(TourBookingCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var booking = await _tourBookingService.BookTourAsync(dto);
                // Ideally return 201 Created
                return Ok(booking);
            }
            catch (Exception ex)
            {
                // Returns 400 if validation fails (e.g. "Only 5 seats available")
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/TourBookings/user/5
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTourBookings(int userId)
        {
            var bookings = await _tourBookingService.GetUserTourBookingsAsync(userId);
            return Ok(bookings);
        }

        // GET: api/TourBookings/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _tourBookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound(new { message = "Booking not found" });
            return Ok(booking);
        }

        // DELETE: api/TourBookings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _tourBookingService.CancelBookingAsync(id);
            if (!result) return NotFound(new { message = "Booking not found or already cancelled" });
            return Ok(new { message = "Tour Booking cancelled and seats released." });
        }

        // GET: api/TourBookings/all (Admin Only)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _tourBookingService.GetAllTourBookingsAsync();
            return Ok(bookings);
        }
    }
}