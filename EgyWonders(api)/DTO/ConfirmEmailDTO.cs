using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class ConfirmEmailDTO
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}
