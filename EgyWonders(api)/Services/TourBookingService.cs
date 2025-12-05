using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EgyWonders.DTO;
using EgyWonders.Models;
using EgyWonders.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EgyWonders.Services
{
    public class TourBookingService : ITourBookingService
    {
        private readonly IUnitOfWork _uow;

        public TourBookingService(IUnitOfWork uow)
        {
            _uow = uow;
        }

      
        public async Task<TourBookingDTO> BookTourAsync(TourBookingCreateDTO dto)
        {
            // A. Get Schedule
            var schedule = (await _uow.Repository<TourSchedule>().GetAllAsync(
                filter: s => s.ScheduleId == dto.ScheduleId,
                includes: x => x.Tour
            )).FirstOrDefault();

            if (schedule == null) throw new Exception("Schedule not found.");

            // B. Capacity Check
            int current = schedule.CurrentBooked ?? 0;
            int available = schedule.MaxParticipants - current;

            if (dto.NumParticipants > available)
                throw new Exception($"Only {available} tickets left.");

            // C. Price Calculation
            decimal pricePerPerson = schedule.Tour.BasePrice ?? 0;
            decimal total = pricePerPerson * dto.NumParticipants;

            // D. Create Booking (Pending)
            var booking = new TourBooking
            {
                UserId = dto.UserId,
                ScheduleId = dto.ScheduleId,
                NumParticipants = dto.NumParticipants,
                TotalPrice = total,
                Status = "Pending Payment" 
            };

            // E. Reserve Seats (Hold inventory immediately)
            // Even though payment isn't done, we must hold the seats so no one else takes them.
            // A background job should release them if payment isn't made within X minutes.
            schedule.CurrentBooked = current + dto.NumParticipants;
            _uow.Repository<TourSchedule>().Update(schedule);

            _uow.Repository<TourBooking>().Add(booking);
            await _uow.CompleteAsync();

            // F. Return Result
            var guest = await _uow.Repository<User>().GetByIdAsync(dto.UserId);

            return new TourBookingDTO
            {
                BookingId = booking.BookingId,
                TourTitle = schedule.Tour.Title,
                GuestName = $"{guest.FirstName} {guest.LastName}",
                StartTime = schedule.StartTime,
                NumParticipants = booking.NumParticipants,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status
            };
        }

       
        // 2. READ Operations
    
        public async Task<TourBookingDTO> GetBookingByIdAsync(int bookingId)
        {
            var booking = (await _uow.Repository<TourBooking>().GetAllAsync(
                filter: b => b.BookingId == bookingId,
                includes: new Expression<Func<TourBooking, object>>[] { x => x.Schedule.Tour, x => x.User }
            )).FirstOrDefault();

            if (booking == null) return null;

            return MapDto(booking);
        }

        public async Task<IEnumerable<TourBookingDTO>> GetAllTourBookingsAsync()
        {
            var bookings = await _uow.Repository<TourBooking>().GetAllAsync(
               null,
               x => x.Schedule.Tour, x => x.User
           );
            return bookings.Select(b => MapDto(b));
        }

        public async Task<IEnumerable<TourBookingDTO>> GetUserTourBookingsAsync(int userId)
        {
            var bookings = await _uow.Repository<TourBooking>().GetAllAsync(
                filter: b => b.UserId == userId,
                includes: new Expression<Func<TourBooking, object>>[] { x => x.Schedule.Tour, x => x.User }
            );
            return bookings.Select(b => MapDto(b));
        }

     
        // 3. CANCEL Operation
        
        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var booking = await _uow.Repository<TourBooking>().GetByIdAsync(bookingId);
            if (booking == null || booking.Status == "Cancelled") return false;

            // Restore seats
            var schedule = await _uow.Repository<TourSchedule>().GetByIdAsync(booking.ScheduleId);
            if (schedule != null)
            {
                int current = schedule.CurrentBooked ?? 0;
                schedule.CurrentBooked = Math.Max(0, current - booking.NumParticipants);
                _uow.Repository<TourSchedule>().Update(schedule);
            }

            booking.Status = "Cancelled";
            _uow.Repository<TourBooking>().Update(booking);
            await _uow.CompleteAsync();
            return true;
        }

        private TourBookingDTO MapDto(TourBooking b)
        {
            return new TourBookingDTO
            {
                BookingId = b.BookingId,
                TourTitle = b.Schedule.Tour.Title,
                GuestName = $"{b.User.FirstName} {b.User.LastName}",
                StartTime = b.Schedule.StartTime,
                NumParticipants = b.NumParticipants,
                TotalPrice = b.TotalPrice,
                Status = b.Status
            };
        }
    }
}