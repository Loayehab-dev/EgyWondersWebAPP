namespace MVC_front_End_.Models
{
    public class TourResponseDTO
    {
        public int TourId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? CityName { get; set; }
        public decimal? CityLatitude { get; set; }
        public decimal? CityLongitude { get; set; }
        public decimal? BasePrice { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }
}
