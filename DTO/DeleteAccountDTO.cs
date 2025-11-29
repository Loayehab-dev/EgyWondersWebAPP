using System.ComponentModel.DataAnnotations;

namespace EgyWonders.DTO
{
    public class DeleteAccountDTO
    {
        [Required(ErrorMessage = "Password is required to delete account")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmation is required")]
        
        public bool ConfirmDeletion { get; set; }
    }
}
