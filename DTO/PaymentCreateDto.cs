using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class PaymentCreateDto
    {
      [ Required]
        public int BookingId { get; set; } // Can be ListingBooking ID or TourBooking ID

        [Required]
        [Range(1, 1000000, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Credit Card"; // e.g. "Cash", "Visa"

        // In a real app, i  will send a "StripeToken" here
    }
}
