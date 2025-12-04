namespace EgyWonders.DTO
{
    public class PaymentCreateDto
    {
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }

        
        public string TransactionId { get; set; }
    }
}