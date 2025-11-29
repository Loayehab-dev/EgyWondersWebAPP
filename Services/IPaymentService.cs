using EgyWonders.DTO;

namespace EgyWonders.Services
{
    public interface IPaymentService
    {
        // Process payment for a Listing Booking
        Task<PaymentDto> PayForListingBookingAsync(PaymentCreateDto dto);

        // Get payment history for a booking
        Task<IEnumerable<PaymentDto>> GetPaymentsByBookingIdAsync(int bookingId);
    }
}
