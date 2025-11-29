namespace EgyWonders.DTO
{
    public class PaymentDto
    {

        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime PaymentDate { get; set; }
    }
}
