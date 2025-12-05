using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class RegisterDTO
    {
       
            [Required]
            [EmailAddress]
            public string Email { get; set; } = null!;

            [Required]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
            public string Password { get; set; } = null!;

            [Required]
            public string FirstName { get; set; } = null!;

            [Required]
            public string LastName { get; set; } = null!;
            [Required] public string Username { get; set; } = null!;
            [Phone]
            public string? Phone { get; set; }

            public string? Nationality { get; set; }

            public string? Gender { get; set; } // "Male" or "Female"

            public DateOnly? DateOfBirth { get; set; }

            // Role: "Host", "Guest", o"TourGuide" or admin
            [Required]
            [RegularExpression("^(Host|Guest|TourGuide|Admin)$", ErrorMessage = "Role must be 'Host', 'Guest',  'TourGuide' or Admin")]
            public string Role { get; set; } = "Guest";
        }
    }

