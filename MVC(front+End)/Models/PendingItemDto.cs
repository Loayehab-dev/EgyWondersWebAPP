namespace MVC_front_End_.Models
{
    public class PendingItemDto
    {
        public int Id { get; set; } // DocumentId or ListingId
        public string Title { get; set; } // User Name or Listing Title
        public string Type { get; set; } // "Host Document" or "New Listing"
        public string Date { get; set; }
        public string ImageUrl { get; set; } // Avatar or Listing Image
    }
}
