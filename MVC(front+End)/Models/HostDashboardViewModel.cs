namespace MVC_front_End_.Models
{
    public class HostDashboardViewModel
    {
        public decimal TotalEarnings { get; set; }
        public int TotalBookings { get; set; }
        public int TotalListings { get; set; }
        public double AverageRating { get; set; }

        public List<HostBookingVM> RecentBookings { get; set; } = new List<HostBookingVM>();
        public List<ListingDTO> Listings { get; set; } = new List<ListingDTO>();
    }
}
