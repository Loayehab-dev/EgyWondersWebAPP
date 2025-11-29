using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EgyWonders.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // POST: api/Payments/listing
        [HttpPost("listing")]
        public async Task<IActionResult> PayListing(PaymentCreateDto dto)
        {
            try
            {
                var result = await _paymentService.PayForListingBookingAsync(dto);
                return Ok(new { message = "Payment successful", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/Payments/history/5
        [HttpGet("history/{bookingId}")]
        public async Task<IActionResult> GetHistory(int bookingId)
        {
            var payments = await _paymentService.GetPaymentsByBookingIdAsync(bookingId);
            return Ok(payments);
        }
    }
}