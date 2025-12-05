
using MVC_front_End_.Models;

namespace MVC_front_End_.Services
{
    public interface IAdminService
    {
        Task<List<ListingDTO>> GetAllListingsAsync();
        Task<bool> ApproveDocumentAsync(int documentId);
        Task<List<HostDocumentDTO>> GetPendingDocumentsAsync();
        Task<bool> RejectDocumentAsync(int documentId);
        Task<ListingDTO> GetListingByIdAsync(int id);
        Task<bool> DeleteListingAsync(int id);
        Task<bool> UpdateListingStatusAsync(int id, string newStatus);
        Task<DashboardStatsViewModel> GetDashboardStatsAsync();
        Task<List<UserDTO>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(string id);
        Task<List<BookingAdminDTO>> GetAllBookingsAsync();
        Task<List<ReviewAdminDTO>> GetAllReviewsAsync();
        Task<bool> DeleteReviewAsync(int id);
    }
}
