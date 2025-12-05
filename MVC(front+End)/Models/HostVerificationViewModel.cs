namespace MVC_front_End_.Models
{
    public class HostVerificationViewModel
    {
        public int DocumentId { get; set; }
        public int UserId { get; set; }
        public string ApplicantName { get; set; } // We add the name here!
        public string NationalId { get; set; }
        public string DocumentPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
