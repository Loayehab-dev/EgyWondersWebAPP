using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EgyWonders.DTO;
using EgyWonders.Models;
using EgyWonders.Interfaces;

namespace EgyWonders.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        public PaymentService(IUnitOfWork uow, IEmailService emailService)
        {
            _uow = uow;
            _emailService = emailService;
        }
        public async Task<PaymentDto> PayForListingBookingAsync(PaymentCreateDto dto)
        {
            // 1. Get Booking
            var booking = await _uow.Repository<ListingBooking>().GetByIdAsync(dto.BookingId);
            if (booking == null) throw new Exception("Booking not found.");

            // 2. Check Status
            if (booking.Status == "Confirmed" || booking.Status == "Paid")
                return new PaymentDto { Status = booking.Status, Amount = booking.TotalPrice, BookingId = booking.BookId };

            // 3. Relaxed Amount Check (Prevents decimal errors)
            if (Math.Abs(dto.Amount - booking.TotalPrice) > 0.1m)
                throw new Exception($"Amount mismatch. Expected: {booking.TotalPrice}, Got: {dto.Amount}");

            // 4. Create Payment Record
            var payment = new Payment
            {
                BookingId = dto.BookingId, // Maps to BookID
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,

                // ★ IMPORTANT: Save the Transaction ID here! ★
                TransactionId = dto.TransactionId ?? "Manual",

                PaymentDate = DateTime.UtcNow,
                Status = "Completed"
            };

            // 5. Update Status
            booking.Status = "Confirmed";

            // 6. Save to DB
            _uow.Repository<ListingBooking>().Update(booking);
            _uow.Repository<Payment>().Add(payment);
            await _uow.CompleteAsync();

            // 7. Send Email (Safe Mode)
            try
            {
                // Fetch user/listing manually to avoid null reference crashes
                var user = await _uow.Repository<User>().GetByIdAsync(booking.UserId);
                var listing = await _uow.Repository<Listing>().GetByIdAsync(booking.ListingId);

                if (user != null && listing != null)
                {
                    await SendConfirmationEmail(
                        user.Email, user.FirstName, listing.Title, dto.Amount,
                        $"From {booking.CheckIn:yyyy-MM-dd} To {booking.CheckOut:yyyy-MM-dd}",
                        $"Guests: {booking.NumberOfGuests}"
                    );
                }
            }
            catch { /* Ignore email errors */ }

            return new PaymentDto
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                Amount = payment.Amount,
                Method = payment.PaymentMethod,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate
            };
        }
        public async Task<IEnumerable<PaymentDto>> GetPaymentsByBookingIdAsync(int bookingId)
        {
            var payments = await _uow.Repository<Payment>().GetAllAsync(p => p.BookingId == bookingId);

            return payments.Select(p => new PaymentDto
            {
                PaymentId = p.PaymentId,
                BookingId = p.BookingId ,
                Amount = p.Amount,
                Method = p.PaymentMethod,
                Status = p.Status,
                PaymentDate = p.PaymentDate
            });
        }
        private async Task SendConfirmationEmail(string email, string name, string product, decimal amount, string dateInfo, string details)
        {
            if (string.IsNullOrEmpty(email)) return;

            string subject = "Booking Confirmed - EgyWonders";
            string body = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd;'>
                    <h2 style='color: #2c3e50;'>Booking Confirmed! ✅</h2>
                    <p>Hello <strong>{name}</strong>,</p>
                    <p>We have successfully received your payment of <strong>{amount:C}</strong>.</p>
                    <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 10px 0;'>
                        <p><strong>Product:</strong> {product}</p>
                        <p><strong>Schedule:</strong> {dateInfo}</p>
                        <p><strong>Details:</strong> {details}</p>
                    </div>
                    <p>Thank you for using EgyWonders!</p>
                </div>";

            try
            {
                await _emailService.SendEmailAsync(email, subject, body);
            }
            catch
            {
                // Log error internally, but don't fail the payment transaction
            }
        }
    }
}