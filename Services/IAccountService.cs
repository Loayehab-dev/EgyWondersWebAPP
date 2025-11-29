using EgyWonders.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using static EgyWonders.DTO.RegisterDTO;

namespace EgyWonders.Interfaces
{
    public interface IAccountService
    {
        // Auth
        Task<AuthResponseDto> RegisterAsync(RegisterDTO dto);
        Task<AuthResponseDto> LoginAsync(LoginDTO dto);
        Task<AuthResponseDto> RefreshTokenAsync(string token);
        Task ChangePasswordAsync(string userId, ChangePasswordDTO dto);

        // Profile
        Task<UserProfileDTO> GetProfileAsync(string userId);
        Task UpdateProfileAsync(string userId, UpdateProfileDTO dto);
        Task DeleteAccountAsync(string userId, string password);

        // Admin
        Task<IEnumerable<UserProfileDTO>> GetAllUsersAsync();
        Task<UserProfileDTO> GetUserByIdAsync(int businessId);
        Task AssignRoleAsync(AssignRoleDTO dto);
        Task RemoveRoleAsync(AssignRoleDTO dto);
        Task ToggleUserStatusAsync(int businessId, bool isActive);
        Task<string> ConfirmEmailAsync(string userId, string token);
        Task<string> ForgotPasswordAsync(string email);
        Task<string> ResetPasswordAsync(ResetPasswordDTO dto);
        Task<AuthResponseDto> GoogleLoginAsync(string googleIdToken);
         
    }
}
