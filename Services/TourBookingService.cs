using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EgyWonders.DTO;
using EgyWonders.Models;
using EgyWonders.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            // A. Get Schedule + Tour
            var schedule = (await _uow.Repository<TourSchedule>().GetAllAsync(
                filter: s => s.ScheduleId == dto.ScheduleId,
                includes: x => x.Tour
            )).FirstOrDefault();

            if (schedule == null) throw new Exception("Schedule not found.");

            // B. CAPACITY CHECK
            //  (MaxParticipants - CurrentBooked) 
            int current = schedule.CurrentBooked ?? 0;
            int available = schedule.MaxParticipants - current;

            if (dto.NumParticipants > available)
                throw new Exception($"Only {available} tickets left.");


            decimal pricePerPerson = schedule.Tour.BasePrice ?? 0;
            decimal total = pricePerPerson * dto.NumParticipants;

            var booking = new TourBooking
            {
                UserId = dto.UserId,
                ScheduleId = dto.ScheduleId,
                NumParticipants = dto.NumParticipants,
                TotalPrice = total,
                Status = "Confirmed"
            };

            //  Update Inventory
            schedule.CurrentBooked = current + dto.NumParticipants;
            _uow.Repository<TourSchedule>().Update(schedule);


            _uow.Repository<TourBooking>().Add(booking);
            await _uow.CompleteAsync();


            return await GetBookingByIdAsync(booking.BookingId);
        }

        // 2. GET BY ID
        public async Task<TourBookingDTO> GetBookingByIdAsync(int bookingId)
        {
            var booking = (await _uow.Repository<TourBooking>().GetAllAsync(
                filter: b => b.BookingId == bookingId,
                includes: new System.Linq.Expressions.Expression<Func<TourBooking, object>>[]
                {
                    x => x.Schedule.Tour,
                    x => x.User
                }
            )).FirstOrDefault();

            if (booking == null) return null;

            return new TourBookingDTO
            {
                BookingId = booking.BookingId,
                TourTitle = booking.Schedule.Tour.Title,
                GuestName = $"{booking.User.FirstName} {booking.User.LastName}",
                StartTime = booking.Schedule.StartTime,
                NumParticipants = booking.NumParticipants,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status
            };
        }

        // 3. GET MY BOOKINGS
        public async Task<IEnumerable<TourBookingDTO>> GetUserTourBookingsAsync(int userId)
        {
            var bookings = await _uow.Repository<TourBooking>().GetAllAsync(
                filter: b => b.UserId == userId,
                includes: new System.Linq.Expressions.Expression<Func<TourBooking, object>>[]
                {
                    x => x.Schedule.Tour,
                    x => x.User
                }
            );

            return bookings.Select(b => new TourBookingDTO
            {
                BookingId = b.BookingId,
                TourTitle = b.Schedule.Tour.Title,
                GuestName = $"{b.User.FirstName} {b.User.LastName}",
                StartTime = b.Schedule.StartTime,
                NumParticipants = b.NumParticipants,
                TotalPrice = b.TotalPrice,
                Status = b.Status
            });
        }


        // 4. CANCEL
        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var booking = await _uow.Repository<TourBooking>().GetByIdAsync(bookingId);
            if (booking == null) return false;
            if (booking.Status == "Cancelled") return true;

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
    
    public async Task<IEnumerable<TourBookingDTO>> GetAllTourBookingsAsync()
        {
            // Fetch ALL bookings, including related Tour and User data
            var bookings = await _uow.Repository<TourBooking>().GetAllAsync(
                filter: null, // No filter - gets everything
                includes: new System.Linq.Expressions.Expression<Func<TourBooking, object>>[]
                {
                    x => x.Schedule.Tour,
                    x => x.User
                }
            );

            return bookings.Select(b => new TourBookingDTO
            {
                BookingId = b.BookingId,
                TourTitle = b.Schedule.Tour.Title,
                GuestName = $"{b.User.FirstName} {b.User.LastName}",
                StartTime = b.Schedule.StartTime,
                NumParticipants = b.NumParticipants,
                TotalPrice = b.TotalPrice,
                Status = b.Status
            });
        }
    }
    }