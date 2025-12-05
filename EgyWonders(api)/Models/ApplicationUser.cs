using Microsoft.AspNetCore.Identity;

namespace EgyWonders.Models
{
    public class ApplicationUser: IdentityUser

    {
        public string? Gender { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? Phone { get; set; }

        public string? Nationality { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        

        public string? EmailConfirmationToken { get; set; }

      
        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiry { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public bool? IsActive { get; set; }


        public virtual User? User { get; set; }
    }


}

