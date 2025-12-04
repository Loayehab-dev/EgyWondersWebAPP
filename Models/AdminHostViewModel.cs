namespace EgyWonders.Models
{
    public class AdminHostViewModel
    {
        public int DocumentId { get; set; }
        public string ApplicantName { get; set; } // The missing piece!
        public string NationalId { get; set; }
        public string DocumentPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
