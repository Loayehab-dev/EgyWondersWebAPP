namespace EgyWonders.DTO
{
    public class BookingDTO
    {
        public int BookId { get; set; }
        public string ListingTitle { get; set; } // Show title
        public string GuestName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }
}
