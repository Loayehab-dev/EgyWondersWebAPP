using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class TourBookingCreateDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ScheduleId { get; set; }

        [Required]
        [Range(1, 20)]
        public int NumParticipants { get; set; }
    }
}
