namespace EgyWonders.DTO
{
    public class TourScheduleDTO
    {
        public int ScheduleId { get; set; }
        public DateOnly Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxParticipants { get; set; }
        public int CurrentBooked { get; set; }
        public int AvailableSeats => MaxParticipants - CurrentBooked;

    }
}
