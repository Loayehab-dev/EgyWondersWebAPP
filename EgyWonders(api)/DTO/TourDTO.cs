namespace EgyWonders.DTO
{
    public class TourDTO
    {
        public int TourId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string CityName { get; set; }
        public decimal BasePrice { get; set; } 
        public int UserId { get; set; } 

        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        public List<string> PhotoUrls { get; set; } = new List<string>();

        // ★ NEW: List of Available Schedules ★
        public List<TourScheduleDTO> Schedules { get; set; } = new List<TourScheduleDTO>();
    }
}
