using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class GuideCertificationBaseDto
    {
        [Required]
        public int GuideId { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "Certification name cannot exceed 150 characters.")]
        public string CertificationName { get; set; } = null!;

        [Required]
        public DateOnly IssueDate { get; set; }

        [Required]
        public DateOnly ExpiryDate { get; set; }
    }
}
