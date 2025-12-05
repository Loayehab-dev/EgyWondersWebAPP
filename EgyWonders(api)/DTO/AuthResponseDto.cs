using EgyWonders.Models;

namespace EgyWonders.DTO
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        // Very Important: This is the INT ID used for bookings/listings
        // The Frontend needs this to call /api/listings?userId=5
        public int BusinessUserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string UserId { get; set; } = null!;



    }
}
