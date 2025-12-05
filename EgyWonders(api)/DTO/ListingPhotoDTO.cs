using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
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
