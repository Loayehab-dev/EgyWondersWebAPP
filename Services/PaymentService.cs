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

        public PaymentService(IUnitOfWork uow)
        {
            _uow = uow;
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
    }
}