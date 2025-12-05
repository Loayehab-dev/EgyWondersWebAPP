namespace MVC_front_End_.Models
{
    public class ReviewAdminDTO
    {
        public int ReviewId { get; set; }
        public string ReviewerName { get; set; }
        public string ListingTitle { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
