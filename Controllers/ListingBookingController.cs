using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EgyWonders.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ListingBookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public ListingBookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookingCreateDTO dto)
        {
            try
            {
                var booking = await _bookingService.CreateBookingAsync(dto);
                // Return 200 OK with the booking details (price, dates, status)
                return Ok(booking);
            }
            catch (System.Exception ex)
            {
                // Returns 400 if validation fails (e.g. "Already booked")
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserBookings(int userId)
        {
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _bookingService.CancelBookingAsync(id);
            if (!result) return NotFound("Booking not found");
            return Ok("Booking cancelled successfully");
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BookingUpdateDTO dto)
        {
            try
            {
                var result = await _bookingService.UpdateBookingAsync(id, dto);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}