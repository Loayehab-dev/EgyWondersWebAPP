using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class ReviewCreateDTO
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        [Required]
        public int UserId { get; set; } 

        // You must provide EITHER ListingId OR TourId (but not both/neither)
        public int? ListingId { get; set; }
        public int? TourId { get; set; }
    }
}
