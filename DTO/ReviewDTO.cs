namespace EgyWonders.DTO
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string GuestName { get; set; } = null!; // "Ahmed Ali"
        public string TargetName { get; set; } = null!; // "Luxury Villa" or "Pyramids Tour"
    }
}

