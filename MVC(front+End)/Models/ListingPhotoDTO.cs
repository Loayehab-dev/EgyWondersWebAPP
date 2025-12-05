using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class ListingPhotoDTO
    {
        public int PhotoId { get; set; }
        [Required]
        [Url(ErrorMessage = "Please provide a valid URL")]
        public string Url { get; set; } = null!;

        [MaxLength(255)]
        public string? Caption { get; set; }
    }
}
