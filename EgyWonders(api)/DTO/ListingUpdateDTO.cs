namespace EgyWonders.DTO
{
    public class ListingUpdateDTO
    {
        public string? Title { get; set; }
        public decimal? PricePerNight { get; set; }
        public string? CityName { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public int? Capacity { get; set; }

        public List<IFormFile> NewPhotos { get; set; } = new List<IFormFile>();

       
        // If null, we don't change amenities. If empty list [], we remove all.
        public List<int>? AmenityIds { get; set; }
    }
}
