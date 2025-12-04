namespace EgyWonders.DTO
{
    public class BookingAdminDTO
    {

        public int BookingId { get; set; }
        public string ListingTitle { get; set; }
        public string GuestName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } // e.g., "Confirmed", "Pending", "Cancelled"
        public DateTime CreatedAt { get; set; }
    }
}
