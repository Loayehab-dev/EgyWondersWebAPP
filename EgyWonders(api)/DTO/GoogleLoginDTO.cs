using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class GoogleLoginDTO
    {
        [Required] public string IdToken { get; set; } // Token from React/Mobile
    }
}
