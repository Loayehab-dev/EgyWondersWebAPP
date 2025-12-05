using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EgyWonders.Services
{
    public class HostDashboardService : IHostDashbordService
    {
        private readonly IUnitOfWork _uow;

        public HostDashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<HostStatsDTO> GetHostStatsAsync(int hostUserId)
        {
            // 1. Get Listings owned by this Host
            var myListings = await _uow.Repository<Listing>().GetAllAsync(l => l.UserId == hostUserId);

            // If no listings, return empty
            if (!myListings.Any()) return new HostStatsDTO();

            var myListingIds = myListings.Select(l => l.ListingId).ToList();

            // 2. Get Bookings (Using ListingBooking entity based on your code)
            var allBookings = await _uow.Repository<ListingBooking>().GetAllAsync();
            var myBookings = allBookings.Where(b => myListingIds.Contains(b.ListingId)).ToList();

            // 3. Get Reviews
            var allReviews = await _uow.Repository<Review>().GetAllAsync();
            var myReviews = allReviews.Where(r => r.ListingId.HasValue && myListingIds.Contains(r.ListingId.Value)).ToList();

            // 4. Calculate Stats
            var stats = new HostStatsDTO
            {
                TotalListings = myListings.Count(),

                // Sum earnings (Confirmed/Completed only)
                TotalEarnings = myBookings
                    .Where(b => b.Status != null && (b.Status.ToLower() == "confirmed" || b.Status.ToLower() == "completed"))
                    .Sum(b => b.TotalPrice),

                TotalBookings = myBookings.Count(),

                AverageRating = myReviews.Any()
                    ? myReviews.Average(r => (double)r.Rating)
                    : 0.0
            };

            var allPhotos = await _uow.Repository<ListingPhoto>().GetAllAsync();
            var myPhotos = allPhotos.Where(p => myListingIds.Contains(p.ListingId)).ToList();
            // 5. Populate "Your Listings" List (Crucial for Dashboard UI)
            // We map the Entity to the DTO
            stats.Listings = myListings.Select(l => new ListingDTO
            {
                ListingId = l.ListingId,
                Title = l.Title,
                PricePerNight = l.PricePerNight,
                Category = l.Category,
                Status = l.Status ?? "Pending",
                CityName = l.CityName,
        
                UserId = l.UserId,
                Photos = myPhotos
        .Where(p => p.ListingId == l.ListingId)
        .Select(p => new ListingPhotoDTO { Url = p.Url })
        .ToList()

            }).ToList();

            // 6. Populate Recent Bookings (Top 5)
            // Sorted by CheckIn because CreatedAt doesn't exist
            var recentBookings = myBookings
                .OrderByDescending(b => b.CheckIn)
                .Take(5)
                .ToList();

            foreach (var b in recentBookings)
            {
                // Fetch Guest Name
                var guest = await _uow.Repository<User>().GetByIdAsync(b.UserId);

                // Fetch Listing Title
                var listing = myListings.FirstOrDefault(l => l.ListingId == b.ListingId);

                stats.RecentBookings.Add(new HostBookingDTO
                {
                    GuestName = guest != null ? $"{guest.FirstName} {guest.LastName}" : "Guest",
                    ListingTitle = listing != null ? listing.Title : "Unknown",
                    CheckInDate = b.CheckIn.ToShortDateString(),
                    Price = b.TotalPrice,
                    Status = b.Status ?? "Pending"
                });
            }

            return stats;
        }
    }
}