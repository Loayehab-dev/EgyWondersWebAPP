using EgyWonders.DTO;

namespace EgyWonders.Services
{
    public interface IBookingService
    {
        Task<BookingDTO> CreateBookingAsync(BookingCreateDTO dto);
        Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(int userId);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<BookingDTO> UpdateBookingAsync(int bookingId, BookingUpdateDTO dto);
    }
}
