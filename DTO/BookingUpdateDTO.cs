using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class BookingUpdateDTO
    {
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        [Range(1, 100, ErrorMessage = "Guests must be at least 1")]
        public int? NumberOfGuests { get; set; }
    }
}
