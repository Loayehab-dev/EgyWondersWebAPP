using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class UploadGuideCertificationDTO
    {

        [Required]
        public IFormFile File { get; set; }  // the uploaded document
        public string CertificationName { get; set; } = null!;

        public DateOnly IssueDate { get; set; }

        public DateOnly ExpiryDate { get; set; }

    }
}
