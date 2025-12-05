// DTO/GuideCertificationDtos.cs
using MVC_front_End_.Models;
using System.ComponentModel.DataAnnotations;

public class GuideCertificationCreateDto : GuideCertificationBaseDto
{
    [Required]
    public IFormFile Document { get; set; } = null!; // The actual file input
}