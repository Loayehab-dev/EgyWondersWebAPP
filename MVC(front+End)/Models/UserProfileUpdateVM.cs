using System;
using System.ComponentModel.DataAnnotations;

namespace MVC_front_End_.Models
{
    public class UserProfileUpdateVM
    {
        // This ID is crucial for identifying the user on the API side
        public int BusinessUserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Phone]
        public string Phone { get; set; }

        public string Nationality { get; set; }

        public string Gender { get; set; }

        // Use DateTime? for compatibility with HTML date input
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }
    }
}