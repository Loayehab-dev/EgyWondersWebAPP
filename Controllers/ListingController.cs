using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.Tasks;

namespace EgyWonders.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        private readonly IListingService _listingService;

        public ListingsController(IListingService listingService)
        {
            _listingService = listingService;
        }

        [HttpGet]
        [EnableRateLimiting("Fixed")]
        public async Task<IActionResult> GetAll()
        {
            var listings = await _listingService.GetAllListingsAsync();
            return Ok(listings);
        }
        [EnableRateLimiting("Fixed")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var listing = await _listingService.GetListingByIdAsync(id);
            if (listing == null) return NotFound(new { message = "Listing not found" });
            return Ok(listing);
        }

        //  [FromForm] is mandatory for File Uploads
        [HttpPost]
        [HttpPost]
        [Authorize(Roles = "Host, Admin")]
        public async Task<IActionResult> Create([FromForm] ListingCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var created = await _listingService.CreateListingAsync(dto);
                return Ok(created); 
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

       
        [HttpPut("{id}")]
        [Authorize(Roles = "Host, Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] ListingUpdateDTO dto)
        {
            try
            {
                var updated = await _listingService.UpdateListingAsync(id, dto);
                return Ok(updated);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Host, Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _listingService.DeleteListingAsync(id);
                if (!deleted) return NotFound(new { message = "Listing not found" });
                return Ok(new { message = "Listing deleted successfully" });
            }
            catch (System.Exception ex)
            {
                // Catches the "Has Bookings" error
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}