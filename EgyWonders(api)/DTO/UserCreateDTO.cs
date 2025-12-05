namespace EgyWonders.DTO
{
    public class UserCreateDTO
    {
        public string Email { get; set; }
        public string Password { get; set; } 
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string Nationality { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
