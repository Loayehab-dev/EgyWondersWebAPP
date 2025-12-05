using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class ListingCreateDTO
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public decimal PricePerNight { get; set; }

        public string? CityName { get; set; }
        public string? Category { get; set; }
        public int Capacity { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public decimal CityLongitude { get; set; }

        [Required]
        public decimal CityLatitude { get; set; }
        public List<IFormFile> Photos { get; set; } = new List<IFormFile>();

        public List<int> AmenityIds { get; set; } = new List<int>();
    }
}
