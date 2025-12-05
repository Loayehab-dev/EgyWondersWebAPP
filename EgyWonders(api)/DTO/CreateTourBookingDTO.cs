using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class CreateTourBookingDTO
    {
        [Required]
        public int UserId { get; set; } // The Guest

        [Required]
        public int ScheduleId { get; set; } 

        [Required]
        [Range(1, 10, ErrorMessage = "You must book at least 1 ticket.")]
        public int TicketCount { get; set; }
        public decimal TotalPrice { get; internal set; }
        public int NumParticipants { get; internal set; }
    }
}
