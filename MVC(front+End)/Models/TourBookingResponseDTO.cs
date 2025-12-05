namespace MVC_front_End_.Models
{
    public class TourBookingResponseDTO
    {
        public int BookingId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public int NumParticipants { get; set; }

        public int UserId { get; set; }

        public int ScheduleId { get; set; }
    }
}
