namespace EgyWonders.DTO
{
    public class HostStatsDTO
    {
        public decimal TotalEarnings { get; set; }
        public int TotalBookings { get; set; }
        public int TotalListings { get; set; }
        public double AverageRating { get; set; }

        // This list holds the data for the "Recent Bookings" table
        public List<ListingDTO> Listings { get; set; } = new List<ListingDTO>();
        public List<HostBookingDTO> RecentBookings { get; set; } = new List<HostBookingDTO>();
    }
}
