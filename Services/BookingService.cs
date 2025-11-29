using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EgyWonders.DTO;
using EgyWonders.Models;
using EgyWonders.Interfaces;

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
           
            if (dto.CheckIn >= dto.CheckOut)
                throw new Exception("Check-out date must be after check-in date.");

            if (dto.CheckIn.Date < DateTime.UtcNow.Date)
                throw new Exception("Cannot book dates in the past.");

            var listing = await _uow.Repository<Listing>().GetByIdAsync(dto.ListingId);
            if (listing == null)
                throw new Exception("Listing not found.");

            // Check if user is booking their own listing 
            if (listing.UserId == dto.UserId)
                throw new Exception("You cannot book your own listing.");

            
            
            if (listing.Capacity > 0 && dto.NumberOfGuests > listing.Capacity)
            {
                throw new Exception($"This listing has a maximum capacity of {listing.Capacity} guests. You requested {dto.NumberOfGuests}.");
            }

           
            // Check if any confirmed booking overlaps with the requested dates
            var existingBookings = await _uow.Repository<ListingBooking>().GetAllAsync(
                b => b.ListingId == dto.ListingId &&
                     b.Status != "Cancelled" && 
                     b.CheckIn < dto.CheckOut &&
                     b.CheckOut > dto.CheckIn
            );

            if (existingBookings.Any())
                throw new Exception("This listing is unavailable for the selected dates.");


            var numberOfDays = (dto.CheckOut.Date - dto.CheckIn.Date).Days;

            if (numberOfDays < 1) numberOfDays = 1; 

            var totalPrice = numberOfDays * listing.PricePerNight;

            var booking = new ListingBooking
            {
                ListingId = dto.ListingId,
                UserId = dto.UserId,
                CheckIn = dto.CheckIn,
                CheckOut = dto.CheckOut,
                TotalPrice = totalPrice,
                NumberOfGuests = dto.NumberOfGuests, 
                Status = "Pending",
                BookingDate = DateTime.UtcNow
            };

            _uow.Repository<ListingBooking>().Add(booking);
            await _uow.CompleteAsync();

       
            var guest = await _uow.Repository<User>().GetByIdAsync(dto.UserId);

            return new BookingDTO
            {
                BookId = booking.BookId,
                ListingTitle = listing.Title,
                GuestName = guest != null ? $"{guest.FirstName} {guest.LastName}" : "Unknown",
                CheckIn = booking.CheckIn,
                CheckOut = booking.CheckOut,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status
            };
        }

      
        public async Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _uow.Repository<ListingBooking>().GetAllAsync(
                filter: b => b.UserId == userId,
                includes:
                [
                    x => x.Listing,
                    x => x.User
                ]
            );

            return bookings.Select(b => new BookingDTO
            {
                BookId = b.BookId,
                ListingTitle = b.Listing.Title,
                GuestName = $"{b.User.FirstName} {b.User.LastName}",
                CheckIn = b.CheckIn,
                CheckOut = b.CheckOut,
                TotalPrice = b.TotalPrice,
                Status = b.Status
            });
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var booking = await _uow.Repository<ListingBooking>().GetByIdAsync(bookingId);
            if (booking == null) return false;


            booking.Status = "Cancelled";
            _uow.Repository<ListingBooking>().Update(booking);

            await _uow.CompleteAsync();
            return true;
        }
        public async Task<BookingDTO> UpdateBookingAsync(int bookingId, BookingUpdateDTO dto)
        {
            //  Get the Booking (Include Listing to check Price/Capacity)
            var booking = (await _uow.Repository<ListingBooking>().GetAllAsync(
                filter: b => b.BookId == bookingId,
                includes: x => x.Listing
            )).FirstOrDefault();

            if (booking == null) throw new Exception("Booking not found.");
            if (booking.Status == "Cancelled") throw new Exception("Cannot edit a cancelled booking.");

            // Check if Booking is in the past (Optional safety)
            if (booking.CheckIn < DateTime.UtcNow.Date)
                throw new Exception("Cannot edit a past booking.");

         
            if (dto.NumberOfGuests.HasValue)
            {
                if (booking.Listing.Capacity > 0 && dto.NumberOfGuests.Value > booking.Listing.Capacity)
                {
                    throw new Exception($"Maximum capacity is {booking.Listing.Capacity}.");
                }
                booking.NumberOfGuests = dto.NumberOfGuests.Value;
            }

           
            // If either date is changing, we must validate availability & price
            if (dto.CheckIn.HasValue || dto.CheckOut.HasValue)
            {
                // Use New Dates if provided, otherwise keep Old Dates
                var newCheckIn = dto.CheckIn ?? booking.CheckIn;
                var newCheckOut = dto.CheckOut ?? booking.CheckOut;

                // A. Validate Dates
                if (newCheckIn >= newCheckOut)
                    throw new Exception("Check-out must be after check-in.");

                if (newCheckIn.Date < DateTime.UtcNow.Date)
                    throw new Exception("Cannot move booking to the past.");

              
                var conflicts = await _uow.Repository<ListingBooking>().GetAllAsync(
                    b => b.ListingId == booking.ListingId &&
                         b.BookId != booking.BookId && 
                         b.Status != "Cancelled" &&
                         b.CheckIn < newCheckOut &&
                         b.CheckOut > newCheckIn
                );

                if (conflicts.Any())
                    throw new Exception("The listing is unavailable for these new dates.");

             
                // I assume price per night didn't change (using the Listing's current price)
                var nights = (newCheckOut.Date - newCheckIn.Date).Days;
                if (nights < 1) nights = 1;

                booking.TotalPrice = nights * booking.Listing.PricePerNight;
                booking.CheckIn = newCheckIn;
                booking.CheckOut = newCheckOut;
            }

           
            _uow.Repository<ListingBooking>().Update(booking);
            await _uow.CompleteAsync();

         
            return new BookingDTO
            {
                BookId = booking.BookId,
                ListingTitle = booking.Listing.Title,
                GuestName = booking.User.FirstName, 
                CheckIn = booking.CheckIn,
                CheckOut = booking.CheckOut,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status
            };
        }
    }
}