using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Models;
using Microsoft.AspNetCore.Identity; // Required for UserManager
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EgyWonders.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager; // 1. Add this field

        // 2. Inject UserManager in the constructor
        public AdminService(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        // --- EXISTING DASHBOARD METHOD ---
        public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
        {
            // ... your existing code for stats ...
            var allUsers = await _uow.Repository<User>().GetAllAsync();
            var allListings = await _uow.Repository<Listing>().GetAllAsync();
            var hostDocs = await _uow.Repository<HostDocument>().GetAllAsync();

            return new DashboardStatsDTO
            {
                TotalUsers = allUsers.Count(),
                TotalListings = allListings.Count(),
                ActiveListings = allListings.Count(l => l.Status?.ToLower() == "active"),
                PendingHostRequests = hostDocs.Count(d => !d.Verified)
            };
        }

        // --- NEW METHOD: GET ALL USERS ---
        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            // 1. Get business users (contains names)
            var businessUsers = await _uow.Repository<User>().GetAllAsync();
            var userDtos = new List<UserDTO>();

            foreach (var bUser in businessUsers)
            {
                if (string.IsNullOrEmpty(bUser.AspNetUserId)) continue;

                // 2. Get Identity user (contains email, roles, phone)
                var identityUser = await _userManager.FindByIdAsync(bUser.AspNetUserId);

                if (identityUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);

                    userDtos.Add(new UserDTO
                    {
                        Id = bUser.AspNetUserId,       // GUID for Identity actions
                        BusinessId = bUser.UserId,     // Int ID for display
                        FullName = bUser.FirstName + " " + bUser.LastName,
                        Email = identityUser.Email,
                        PhoneNumber = identityUser.PhoneNumber,
                        Roles = roles
                    });
                }
            }
            return userDtos;
        }


        public async Task<bool> DeleteUserAsync(string id)
        {
            // 1. Find the Identity User (Login Account)
            var identityUser = await _userManager.FindByIdAsync(id);
            if (identityUser == null) return false;

            // 2. Find the associated Business User (Profile)
            // We need to look up the user where AspNetUserId matches the ID we are deleting
            var businessUsers = await _uow.Repository<User>().GetAllAsync(u => u.AspNetUserId == id);
            var businessUser = businessUsers.FirstOrDefault();

            // 3. Delete the Business User FIRST (if exists)
            if (businessUser != null)
            {
                _uow.Repository<User>().Remove(businessUser);
                await _uow.CompleteAsync(); // Save changes to DB immediately
            }

            // 4. NOW delete the Identity User (Login Account)
            var result = await _userManager.DeleteAsync(identityUser);

            return result.Succeeded;
        }
        public async Task<List<BookingAdminDTO>> GetAllBookingsAsync()
        {
            // 1. Fetch all bookings
            var bookings = await _uow.Repository<ListingBooking>().GetAllAsync();

            var dtoList = new List<BookingAdminDTO>();

            foreach (var b in bookings)
            {
                // 2. Fetch Related Data (Listing & Guest) manually to be safe
                var listing = await _uow.Repository<Listing>().GetByIdAsync(b.ListingId);
                var guest = await _uow.Repository<User>().GetByIdAsync(b.UserId);

                dtoList.Add(new BookingAdminDTO
                {
                    BookingId = b.BookId,
                    ListingTitle = listing != null ? listing.Title : "Unknown Listing",
                    GuestName = guest != null ? $"{guest.FirstName} {guest.LastName}" : "Unknown Guest",
                    CheckInDate = b.CheckIn,
                    CheckOutDate = b.CheckOut,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status ?? "Pending",
                    CreatedAt = b.BookingDate
                });
            }

            // Sort by newest first
            return dtoList.OrderByDescending(x => x.CreatedAt).ToList();
        }
        public async Task<List<ReviewAdminDTO>> GetAllReviewsAsync()
        {
            var reviews = await _uow.Repository<Review>().GetAllAsync();
            var dtoList = new List<ReviewAdminDTO>();

            foreach (var r in reviews)
            {
                string reviewerName = "Unknown User";
                string itemTitle = "Unknown Item"; // Could be a Listing or a Tour

                // 1. Fetch Reviewer Name (Safe check)
                int safeUserId = (int?)r.UserId ?? 0;
                if (safeUserId > 0)
                {
                    var reviewer = await _uow.Repository<User>().GetByIdAsync(safeUserId);
                    if (reviewer != null)
                    {
                        reviewerName = $"{reviewer.FirstName} {reviewer.LastName}";
                    }
                }

                // 2. CHECK: Is this a Listing (Hotel) Review?
                int? listingId = r.ListingId; // Nullable
                if (listingId.HasValue && listingId.Value > 0)
                {
                    var listing = await _uow.Repository<Listing>().GetByIdAsync(listingId.Value);
                    if (listing != null)
                    {
                        itemTitle = $"🏠 {listing.Title}"; 
                    }
                }
                // 3. CHECK: Is this a Tour Review?
                else if (r.TourId.HasValue && r.TourId.Value > 0)
                {
                    // Assuming you have a 'Tour' repository
                    var tour = await _uow.Repository<Tour>().GetByIdAsync(r.TourId.Value);
                    if (tour != null)
                    {
                        // Ensure 'Tour' model has a 'Title' or 'Name' property
                        itemTitle = $"🌍 {tour.Title}";
                    }
                }

                // 4. Map to DTO
                dtoList.Add(new ReviewAdminDTO
                {
                    ReviewId = r.ReviewId,
                    ReviewerName = reviewerName,
                    ListingTitle = itemTitle, // We put the Tour Name here too so it shows in the table
                    Rating = (int?)r.Rating ?? 0,
                    Comment = r.Comment ?? "",
                    CreatedAt = DateTime.Now
                });
            }

            return dtoList.OrderByDescending(r => r.CreatedAt).ToList();
        }
        public async Task<bool> DeleteReviewAsync(int id)
        {
            // 1. Find the review
            var review = await _uow.Repository<Review>().GetByIdAsync(id);

            if (review == null) return false;

            // 2. Remove it
            _uow.Repository<Review>().Remove(review);

            // 3. Save Changes
            await _uow.CompleteAsync();

            return true;
        }
    }
}