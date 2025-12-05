// DTO/GuideCertificationDtos.cs
using EgyWonders.DTO;
using System.ComponentModel.DataAnnotations;

public class GuideCertificationCreateDto : GuideCertificationBaseDto
{
    [Required]
    public IFormFile Document { get; set; } = null!; // The actual file input
}