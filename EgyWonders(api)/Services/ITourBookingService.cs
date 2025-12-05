using EgyWonders.DTO;

namespace EgyWonders.Services
{
    public interface ITourBookingService
    {
        Task<TourBookingDTO> BookTourAsync(TourBookingCreateDTO dto);
        Task<TourBookingDTO> GetBookingByIdAsync(int bookingId);
        Task<IEnumerable<TourBookingDTO>> GetAllTourBookingsAsync();
        Task<IEnumerable<TourBookingDTO>> GetUserTourBookingsAsync(int userId);
        Task<bool> CancelBookingAsync(int bookingId);
    }
}
