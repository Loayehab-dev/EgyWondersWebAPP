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
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _uow;

        public BookingService(IUnitOfWork uow)
        {
            _uow = uow;
        }

      
        public async Task<BookingDTO> CreateBookingAsync(BookingCreateDTO dto)
        {
            if (dto.CheckIn >= dto.CheckOut) throw new Exception("Check-out must be after check-in.");
            if (dto.CheckIn.Date < DateTime.UtcNow.Date) throw new Exception("Cannot book past dates.");

            var listing = await _uow.Repository<Listing>().GetByIdAsync(dto.ListingId);
            if (listing == null) throw new Exception("Listing not found.");

            if (listing.UserId == dto.UserId) throw new Exception("You cannot book your own listing.");

            // Capacity Check
            if (listing.Capacity > 0 && dto.NumberOfGuests > listing.Capacity)
                throw new Exception($"Max capacity is {listing.Capacity}.");

            // Availability Check
            var conflicts = await _uow.Repository<ListingBooking>().GetAllAsync(
                b => b.ListingId == dto.ListingId &&
                     b.Status != "Cancelled" &&
                     b.CheckIn < dto.CheckOut &&
                     b.CheckOut > dto.CheckIn
            );

            if (conflicts.Any()) throw new Exception("Dates are unavailable.");

            // Price Calculation
            var days = (dto.CheckOut.Date - dto.CheckIn.Date).Days;
            if (days < 1) days = 1;
            var total = days * listing.PricePerNight;

            // Save
            var booking = new ListingBooking
            {
                ListingId = dto.ListingId,
                UserId = dto.UserId,
                CheckIn = dto.CheckIn,
                CheckOut = dto.CheckOut,
                NumberOfGuests = dto.NumberOfGuests,
                TotalPrice = total,
                Status = "Pending Payment", 
                BookingDate = DateTime.UtcNow
            };

            _uow.Repository<ListingBooking>().Add(booking);
            await _uow.CompleteAsync();

            // Return DTO
            var guest = await _uow.Repository<User>().GetByIdAsync(dto.UserId);
            return MapToDto(booking, listing.Title, guest);
        }

        
        //  (GetById, GetAll, GetUser)
        

        public async Task<BookingDTO> GetBookingByIdAsync(int bookingId)
        {
            var booking = (await _uow.Repository<ListingBooking>().GetAllAsync(
                filter: b => b.BookId == bookingId,
                includes: new Expression<Func<ListingBooking, object>>[] { x => x.Listing, x => x.User }
            )).FirstOrDefault();

            if (booking == null) return null;

            return MapToDto(booking, booking.Listing.Title, booking.User);
        }

        public async Task<IEnumerable<BookingDTO>> GetAllBookingsAsync()
        {
            var bookings = await _uow.Repository<ListingBooking>().GetAllAsync(
                null,
                x => x.Listing, x => x.User
            );
            return bookings.Select(b => MapToDto(b, b.Listing.Title, b.User));
        }

        public async Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _uow.Repository<ListingBooking>().GetAllAsync(
                filter: b => b.UserId == userId,
                includes: new Expression<Func<ListingBooking, object>>[] { x => x.Listing, x => x.User }
            );
            return bookings.Select(b => MapToDto(b, b.Listing.Title, b.User));
        }

       
        // 3. UPDATE OPERATION
      
        public async Task<BookingDTO> UpdateBookingAsync(int bookingId, BookingUpdateDTO dto)
        {
            var booking = (await _uow.Repository<ListingBooking>().GetAllAsync(
                filter: b => b.BookId == bookingId,
                includes: new Expression<Func<ListingBooking, object>>[] { x => x.Listing, x => x.User }
            )).FirstOrDefault();

            if (booking == null) throw new Exception("Booking not found.");
            if (booking.Status == "Cancelled") throw new Exception("Cannot edit cancelled booking.");
            if (booking.CheckIn < DateTime.UtcNow.Date) throw new Exception("Cannot edit past booking.");

            // Update Guests
            if (dto.NumberOfGuests.HasValue)
            {
                if (booking.Listing.Capacity > 0 && dto.NumberOfGuests.Value > booking.Listing.Capacity)
                    throw new Exception($"Max capacity is {booking.Listing.Capacity}.");
                booking.NumberOfGuests = dto.NumberOfGuests.Value;
            }

            // Update Dates
            if (dto.CheckIn.HasValue || dto.CheckOut.HasValue)
            {
                var newCheckIn = dto.CheckIn ?? booking.CheckIn;
                var newCheckOut = dto.CheckOut ?? booking.CheckOut;

                if (newCheckIn >= newCheckOut) throw new Exception("Invalid date range.");
                if (newCheckIn.Date < DateTime.UtcNow.Date) throw new Exception("Cannot move to past.");

                // Conflict Check (Exclude self)
                var conflicts = await _uow.Repository<ListingBooking>().GetAllAsync(
                    b => b.ListingId == booking.ListingId &&
                         b.BookId != booking.BookId &&
                         b.Status != "Cancelled" &&
                         b.CheckIn < newCheckOut &&
                         b.CheckOut > newCheckIn
                );

                if (conflicts.Any()) throw new Exception("Dates unavailable.");

                // Recalculate Price
                var days = (newCheckOut.Date - newCheckIn.Date).Days;
                if (days < 1) days = 1;

                booking.TotalPrice = days * booking.Listing.PricePerNight;
                booking.CheckIn = newCheckIn;
                booking.CheckOut = newCheckOut;
            }

            _uow.Repository<ListingBooking>().Update(booking);
            await _uow.CompleteAsync();

            return MapToDto(booking, booking.Listing.Title, booking.User);
        }

       
        // 4. CANCEL OPERATION
        
        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var booking = await _uow.Repository<ListingBooking>().GetByIdAsync(bookingId);
            if (booking == null || booking.Status == "Cancelled") return false;

            booking.Status = "Cancelled";
            _uow.Repository<ListingBooking>().Update(booking);
            await _uow.CompleteAsync();
            return true;
        }

        // --- HELPER ---
        private BookingDTO MapToDto(ListingBooking b, string title, User user)
        {
            return new BookingDTO
            {
                BookId = b.BookId,
                ListingTitle = title,
                GuestName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                CheckIn = b.CheckIn,
                CheckOut = b.CheckOut,
                TotalPrice = b.TotalPrice,
                Status = b.Status
            };
        }
    }
}