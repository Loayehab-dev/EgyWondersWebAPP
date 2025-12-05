using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class TourBookingDTO
    {
        public int BookingId { get; set; }
        public string TourTitle { get; set; } = null!;
        public string GuestName { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public int NumParticipants { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }
    }
}
