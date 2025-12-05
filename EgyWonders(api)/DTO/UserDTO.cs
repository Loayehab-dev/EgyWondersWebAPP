namespace EgyWonders.DTO
{
    public class UserDTO
    {

        public string Id { get; set; } // The AspNetUser ID 
        public int BusinessId { get; set; } // The integer ID from 'Users' table
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
