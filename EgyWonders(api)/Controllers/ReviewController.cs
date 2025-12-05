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
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // POST: api/Reviews
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(ReviewCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var created = await _reviewService.CreateReviewAsync(dto);
                return Ok(created);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += " | Inner: " + ex.InnerException.Message;
                }
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/Reviews/listing/5
        [AllowAnonymous]
        [HttpGet("listing/{listingId}")]
        public async Task<IActionResult> GetByListingId(int listingId)
        {
            var reviews = await _reviewService.GetReviewsByListingIdAsync(listingId);
            return Ok(reviews);
        }

        // GET: api/Reviews/tour/10
        [AllowAnonymous]
        [HttpGet("tour/{tourId}")]
        public async Task<IActionResult> GetByTourId(int tourId)
        {
            var reviews = await _reviewService.GetReviewsByTourIdAsync(tourId);
            return Ok(reviews);
        }

        // DELETE: api/Reviews/15
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _reviewService.DeleteReviewAsync(id);
            if (!deleted) return NotFound("Review not found");
            return Ok("Review deleted successfully");
        }
    }
}