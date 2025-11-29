using EgyWonders.Models;

namespace EgyWonders.DTO
{
    public class ListingDTO
    {
        public int ListingId { get; set; }
        public string Title { get; set; }
        public decimal PricePerNight { get; set; }
        public string CityName { get; set; }
        public string ?Status { get; set; }
        public string Category { get; set; }
        public int UserId { get; set; } // The host ID
        public int Capacity { get; set; }


        public List<ListingPhotoDTO> Photos { get; set; }
        public List<string> AmenityNames { get; set; } = new List<string>();
        public List<int> AmenityIds { get; set; } = new List<int>();


    }
}
