using EgyWonders.DTO;

namespace EgyWonders.Services
{
    public interface IAdminService
    {
        Task<DashboardStatsDTO> GetDashboardStatsAsync();
        Task<List<UserDTO>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(string id);
        Task<List<BookingAdminDTO>> GetAllBookingsAsync();
        Task<List<ReviewAdminDTO>> GetAllReviewsAsync();
        Task<bool> DeleteReviewAsync(int id);
    }
}
