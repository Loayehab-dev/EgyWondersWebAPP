namespace MVC_front_End_.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalHosts { get; set; }
        public int TotalListings { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }

        // 2. Pending Approvals (List of people waiting)
        public List<PendingItemDto> PendingApprovals { get; set; }

        // 3. Chart Data
        public List<decimal> RevenueData { get; set; }
    }
}
