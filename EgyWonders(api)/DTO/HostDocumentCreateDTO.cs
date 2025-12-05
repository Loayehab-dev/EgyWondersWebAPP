using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class HostDocumentCreateDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public IFormFile Document { get; set; } = null!; //  The actual file

        [Required, MaxLength(50)]
        public string NationalId { get; set; } = null!;

        public string? TextRecord { get; set; }
    }
}
