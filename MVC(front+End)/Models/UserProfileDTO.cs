namespace MVC_front_End_.Models
{
    public class UserProfileDTO
    {
        public string Id { get; set; } // Identity ID
        public int BusinessUserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Nationality { get; set; }
        public string Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public IList<string> Roles { get; set; }
        public bool IsActive { get; set; }
    }
}
