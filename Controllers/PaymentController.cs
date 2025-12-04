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

        [HttpPost("listing")]
        public async Task<IActionResult> PayListing([FromBody] PaymentCreateDto dto)
        {
            try
            {
                var result = await _paymentService.PayForListingBookingAsync(dto);
                return Ok(new { message = "Payment successful", data = result });
            }
            catch (Exception ex)
            {
              
                var realError = ex;
                while (realError.InnerException != null)
                {
                    realError = realError.InnerException;
                }

                return BadRequest( );
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