using EgyWonders.DTO;

namespace EgyWonders.Services
{
    public interface IBookingService
    {
        Task<BookingDTO> CreateBookingAsync(BookingCreateDTO dto);

  
        Task<BookingDTO> GetBookingByIdAsync(int bookingId);
        Task<IEnumerable<BookingDTO>> GetAllBookingsAsync();

        Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(int userId);
        Task<BookingDTO> UpdateBookingAsync(int bookingId, BookingUpdateDTO dto);
        Task<bool> CancelBookingAsync(int bookingId);
    }
}
