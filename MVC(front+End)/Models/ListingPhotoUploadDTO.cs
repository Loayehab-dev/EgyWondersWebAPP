using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class ListingPhotoUploadDTO
    {
        [Required]
        public IFormFile Photo { get; set; }

        public string? Caption { get; set; }

        [Required]
        public int ListingId { get; set; }
    }
}
