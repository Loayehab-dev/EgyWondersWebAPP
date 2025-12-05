using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class BookingCreateDTO
    {
        [Required]
        public int ListingId { get; set; }

        [Required]
        public int UserId { get; set; } // The Guest ID

        [Required]
        public DateTime CheckIn { get; set; }

        [Required]
        public DateTime CheckOut { get; set; }
        [Required]
        [Range(1, 20, ErrorMessage = "Guests must be between 1 and 20")]
        public int NumberOfGuests { get; set; }

    }

}
