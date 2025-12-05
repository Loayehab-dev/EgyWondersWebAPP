namespace MVC_front_End_.Models
{
    public class AuthResponseDto
    {
        public int BusinessUserId { get; set; }
        public string Message { get; set; }
            public bool IsAuthenticated { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public List<string> Roles { get; set; }
            public string Token { get; set; }
            public string RefreshToken { get; set; }
            public DateTime RefreshTokenExpiration { get; set; }
        public string UserId { get; set; }



    }
}
