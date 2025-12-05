using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class TourBookingUpdateDTO
    {
        public int? NewScheduleId { get; set; }

        [Range(1, 10)]
        public int? TicketCount { get; set; }
    }
}
