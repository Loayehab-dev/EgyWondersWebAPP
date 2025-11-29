using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
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
    }
}
