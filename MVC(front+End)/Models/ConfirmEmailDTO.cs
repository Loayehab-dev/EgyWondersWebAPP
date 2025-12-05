using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models        
{
    public class ConfirmEmailDTO
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}
