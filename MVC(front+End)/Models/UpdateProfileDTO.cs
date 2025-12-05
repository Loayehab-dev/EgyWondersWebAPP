using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class UpdateProfileDTO
    {
        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        [StringLength(50)]
        public string? Nationality { get; set; }

        public DateOnly? DateOfBirth { get; set; }
    }
}
