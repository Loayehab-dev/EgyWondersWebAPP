using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class GoogleLoginDTO
    {
        [Required] public string IdToken { get; set; } // Token from React/Mobile
    }
}
