namespace MVC_front_End_.Models
{
    public class TokenResponseDTO
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
       // public UserInfoDTO User { get; set; } = null!;
    }
}
