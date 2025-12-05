namespace EgyWonders.DTO
{
    public class TourScheduleSummaryDTO
    {
        public int ScheduleId { get; set; }
        public int TourId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int MaxParticipants { get; set; }
        public int? CurrentBooked { get; set; }
        public int AvailableSpots { get; set; }
        public string? TourName { get; set; }
        public bool IsAvailable { get; set; }
    }
}
