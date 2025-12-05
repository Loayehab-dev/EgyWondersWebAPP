namespace MVC_front_End_.Models
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string  UserId { get; set; }  = string.Empty;
        public int BusinessUserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public List<string>? Roles { get; set; }
    }
}
