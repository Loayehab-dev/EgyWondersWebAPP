using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class TourBookingUpdateDTO
    {
        public int? NewScheduleId { get; set; }

        [Range(1, 10)]
        public int? TicketCount { get; set; }
    }
}
