namespace EgyWonders.DTO
{
    public class CreateReviewDTO
    {

        public string? Comment { get; set; }
        public int Rating { get; set; }
        public int UserId { get; set; }
        public int? ListingId { get; set; }
        public int? TourId { get; set; }

    }
}


