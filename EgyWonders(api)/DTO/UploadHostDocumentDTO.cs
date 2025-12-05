using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class UploadHostDocumentDTO
    {
        [Required(ErrorMessage = "File is required.")]
        public IFormFile File { get; set; }  // the uploaded document

        [StringLength(5000, ErrorMessage = "TextRecord can't exceed 5000 characters.")]
        public string? TextRecord { get; set; }

        [RegularExpression(@"^[0-9]{14}$", ErrorMessage = "National ID must be 14 digits.")]
        public string? NationalId { get; set; }
    }
}
