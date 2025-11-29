using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class TourScheduleCreateDTO
    {
        [Required]
        public int TourId { get; set; }

        [Required]
        public DateTime StartTime { get; set; } // 2025-10-01 09:00:00

        [Required]
        public DateTime EndTime { get; set; }   // 2025-10-01 17:00:00

        [Required]
        [Range(1, 500)]
        public int MaxParticipants { get; set; } 
    }
}
