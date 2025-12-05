using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class AssignRoleDTO
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role name is required")]
        public string RoleName { get; set; } = string.Empty;
    }
}
