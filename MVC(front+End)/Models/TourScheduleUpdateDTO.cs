using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class TourScheduleUpdateDTO
    {
        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Max participants must be between 1 and 1000")]
        public int MaxParticipants { get; set; }
    }
}
