using EgyWonders.Interfaces;
using EgyWonders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EgyWonders.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingPhotosController : ControllerBase
    {
        // Notice: We inject IListingService, not a separate PhotoService
        private readonly IListingService _listingService;

        public ListingPhotosController(IListingService listingService)
        {
            _listingService = listingService;
        }

        // DELETE: api/ListingPhotos/5
        // This allows the frontend to have a little "Trash" icon on each photo
        [Authorize(Roles = "Host, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _listingService.DeletePhotoAsync(id);
                if (!result) return NotFound(new { message = "Photo not found" });

                return Ok(new { message = "Photo deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}