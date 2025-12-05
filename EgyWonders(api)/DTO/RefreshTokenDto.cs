using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;
    }
}
