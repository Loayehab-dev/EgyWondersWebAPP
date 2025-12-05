using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class ListingUpdateDTO
    {
        public int ListingId { get; set; } // Helper for the View

        [Required]
        public string Title { get; set; }
       
        public decimal? PricePerNight { get; set; }
        public string CityName { get; set; }
        public decimal CityLatitude { get; set; }
        public decimal CityLongitude { get; set; }
        public string Category { get; set; }
        public int? Capacity { get; set; }
        public string ?Status { get; set; }

       
        public List<IFormFile> NewPhotos { get; set; } = new List<IFormFile>();

        // For Amenities Checkboxes
        public List<int> AmenityIds { get; set; } = new List<int>();

        // For displaying existing data in the View (not sent to API)
        public List<ListingPhotoDTO> ExistingPhotos { get; set; } = new List<ListingPhotoDTO>();
    }
}
