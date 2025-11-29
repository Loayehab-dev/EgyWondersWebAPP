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
            // 1. Validate Booking Exists
            var booking = await _uow.Repository<ListingBooking>().GetByIdAsync(dto.BookingId);
            if (booking == null) throw new Exception("Booking not found.");

            // 2. Validate Status
            if (booking.Status == "Paid" || booking.Status == "Cancelled")
                throw new Exception($"Booking is already {booking.Status}.");

            // 3. Validate Amount (Simple simulation)
            if (dto.Amount != booking.TotalPrice)
                throw new Exception($"Incorrect amount. Expected: {booking.TotalPrice}");

            // 4. Create Payment Record
            var payment = new Payment
            {
                BookingId = dto.BookingId,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                PaymentDate = DateTime.UtcNow,
                Status = "Completed"
            };

            // 5. Update Booking Status
            booking.Status = "Paid";

            // 6. Save Everything
            _uow.Repository<ListingBooking>().Update(booking);
            _uow.Repository<Payment>().Add(payment);

            await _uow.CompleteAsync();
            await SendConfirmationEmail(
                booking.User.Email,
                booking.User.FirstName,
                booking.Listing.Title,
                dto.Amount,
                $"From {booking.CheckIn:yyyy-MM-dd} To {booking.CheckOut:yyyy-MM-dd}",
                $"Guests: {booking.NumberOfGuests}"
            );
            return new PaymentDto
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId ?? 0,
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
                BookingId = p.BookingId ?? 0,
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