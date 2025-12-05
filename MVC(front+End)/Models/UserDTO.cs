namespace MVC_front_End_.Models
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
        public bool IsActive { get; set; } // Useful for Admin dashboard
        public int BusinessId { get; set; } // The readable ID
        public string FullName { get; set; }
    
    }
}
