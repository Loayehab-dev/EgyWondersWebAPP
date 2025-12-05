using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
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
