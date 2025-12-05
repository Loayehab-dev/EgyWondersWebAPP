using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;
    }
}
