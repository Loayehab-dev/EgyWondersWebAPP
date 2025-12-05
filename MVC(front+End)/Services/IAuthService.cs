using MVC_front_End_.Models;

namespace MVC_front_End_.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDTO model);
        Task<AuthResponseDto> RegisterAsync(RegisterDTO model);
        Task<string> ChangePasswordAsync(ChangePasswordDTO model, string token);
        Task<UserDTO> GetProfileAsync(string token);
        Task<List<UserDTO>> GetAllUsersAsync(string token); // Admin
        Task<bool> ToggleUserStatusAsync(int id, bool isActive, string token); // Admin
        Task<string> ForgotPasswordAsync(string email);
        Task<string> ResetPasswordAsync(ResetPasswordDTO model);
        Task<AuthResponseDto> GoogleLoginAsync(string idToken);
        Task<string> ConfirmEmailAsync(string userId, string token);
        Task<bool> RegisterHostAsync(HostDocumentCreateDTO model);
        Task LogoutAsync();
        Task<UserProfileDTO> GetUserProfileAsync(string token);
        Task<bool> UpdateUserProfileAsync(UpdateProfileDTO model, string token);
    }
}
