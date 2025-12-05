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
    public class ListingBookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public ListingBookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // POST: api/ListingBooking
        [HttpPost]
        public async Task<IActionResult> Create(BookingCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var booking = await _bookingService.CreateBookingAsync(dto);
                // Ideally return 201 Created
                return Ok(booking);
            }
            catch (Exception ex)
            {
                // Returns 400 if validation fails (e.g. "Already booked", "Capacity exceeded")
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/ListingBooking/user/5
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserBookings(int userId)
        {
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }

        // GET: api/ListingBooking/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound(new { message = "Booking not found" });
            return Ok(booking);
        }

        // PUT: api/ListingBooking/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BookingUpdateDTO dto)
        {
            try
            {
                var result = await _bookingService.UpdateBookingAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/ListingBooking/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _bookingService.CancelBookingAsync(id);
            if (!result) return NotFound(new { message = "Booking not found or already cancelled" });
            return Ok(new { message = "Booking cancelled successfully" });
        }

        // GET: api/ListingBooking/all (Admin Only)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return Ok(bookings);
        }
    }
}