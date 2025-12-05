using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class HostDocumentDTO
    {
        public int DocumentId { get; set; }
        public int UserId { get; set; }
        public string DocumentPath { get; set; } = null!; // The URL to view the file
        public string NationalId { get; set; } = null!;
        public string? TextRecord { get; set; }
        public bool Verified { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ApplicantName { get; set; }
    }
}
