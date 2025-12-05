namespace MVC_front_End_.Models
{
    public class BookingDTO
    {
        public int BookId { get; set; }

        public string ListingPhotoUrl { get; set; }
        public int ListingId { get; set; }

        public string ListingTitle { get; set; }
        public string GuestName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }
}