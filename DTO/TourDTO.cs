namespace EgyWonders.DTO
{
    public class TourDTO
    {
        public int TourId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public string? CityName { get; set; }
        public List<TourScheduleDTO> Schedules { get; set; } = new List<TourScheduleDTO>();
    }
}
