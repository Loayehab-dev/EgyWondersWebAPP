namespace MVC_front_End_.Models
{
    public class AmenityDTO
    {
        public int AmenitiesId { get; set; }
        public string AmenityName { get; set; } = null!;
        public List<ListingDTO> Listings { get; set; } = new List<ListingDTO>();

    }
}
