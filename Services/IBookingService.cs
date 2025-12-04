using EgyWonders.DTO;
using System.Collections.Generic; // Required for IEnumerable
using System.Threading.Tasks;     // Required for Task

namespace EgyWonders.Services
{
    public interface IBookingService
    {
        Task<BookingDTO> CreateBookingAsync(BookingCreateDTO dto);
        Task<BookingDTO> GetBookingByIdAsync(int bookingId);
        Task<IEnumerable<BookingDTO>> GetAllBookingsAsync();

        // This is the method used by "My Trips"
        Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(int userId);

        Task<BookingDTO> UpdateBookingAsync(int bookingId, BookingUpdateDTO dto);
        Task<bool> CancelBookingAsync(int bookingId);
    }
}