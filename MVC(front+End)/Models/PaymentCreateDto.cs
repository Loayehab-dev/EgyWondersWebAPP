using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models { 
    public class PaymentCreateDto
    {
        [Required] public int BookingId { get; set; }
        [Required] public decimal Amount { get; set; }
        [Required] public string PaymentMethod { get; set; }

        // ★ REQUIRED: Matches the column you added to SQL ★
        public string TransactionId { get; set; }
        // In a real app, i  will send a "StripeToken" here
    }
}
