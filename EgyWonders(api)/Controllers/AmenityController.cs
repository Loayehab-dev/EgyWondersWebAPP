using EgyWonders.DTO;
using EgyWonders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.Tasks;

namespace EgyWonders.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmenitiesController : ControllerBase
    {
        private readonly IAmenityService _amenityService;

        public AmenitiesController(IAmenityService amenityService)
        {
            _amenityService = amenityService;
        }

       
        [HttpGet]
        [EnableRateLimiting("Fixed")]
        public async Task<IActionResult> GetAll()
        {
            var amenities = await _amenityService.GetAllAmenitiesAsync();
            return Ok(amenities);
        }
        [EnableRateLimiting("Fixed")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var amenity = await _amenityService.GetAmenityByIdAsync(id);

            if (amenity == null)
                return NotFound(new { message = "Amenity not found" });

            return Ok(amenity);
        }
       
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AmenityDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdAmenity = await _amenityService.CreateAmenityAsync(dto);

          
            return Ok(createdAmenity);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
               
                bool isDeleted = await _amenityService.DeleteAmenityAsync(id);

                if (!isDeleted)
                {
                    return NotFound(new { message = "Amenity not found" });
                }

                return Ok(new { message = "Amenity deleted successfully" });
            }
            catch (System.Exception ex)
            {
              
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}