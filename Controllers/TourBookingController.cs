using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EgyWonders.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TourBookingsController : ControllerBase
    {
        private readonly ITourBookingService _tourBookingService;

        public TourBookingsController(ITourBookingService tourBookingService)
        {
            _tourBookingService = tourBookingService;
        }

        // POST: api/TourBookings (Book tickets for a schedule)
        [HttpPost]
        public async Task<IActionResult> BookTour(TourBookingCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var booking = await _tourBookingService.BookTourAsync(dto);
                return Ok(booking);
            }
            catch (Exception ex)
            {
                // Catches errors like "Only 5 seats available"
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/TourBookings/5 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _tourBookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound(new { message = "Booking not found" });
            return Ok(booking);
        }

        // GET: api/TourBookings/user/101 (Get all tours booked by a specific user)
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetMyTours(int userId)
        {
            var bookings = await _tourBookingService.GetUserTourBookingsAsync(userId);
            return Ok(bookings);
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _tourBookingService.GetAllTourBookingsAsync();
            return Ok(bookings);
        }

        // DELETE: api/TourBookings/5 (Cancel booking and restore seats)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var success = await _tourBookingService.CancelBookingAsync(id);
            if (!success) return NotFound(new { message = "Booking not found" });
            return Ok(new { message = "Tour Booking cancelled and seats released." });
        }
    }
}