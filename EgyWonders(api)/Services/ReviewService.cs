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
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _uow;

        public ReviewService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ReviewDTO> CreateReviewAsync(ReviewCreateDTO dto)
        {
            // 1. Validation: Ensure we are reviewing exactly ONE thing (XOR Check)
            if ((dto.ListingId == null && dto.TourId == null) || (dto.ListingId != null && dto.TourId != null))
            {
                throw new Exception("A review must be linked to either a Listing OR a Tour (not both, not neither).");
            }

            // 2. Map DTO to Entity
            var review = new Review
            {
                Rating = dto.Rating,
                Comment = dto.Comment,
                UserId = dto.UserId,
                ListingId = dto.ListingId,
                TourId = dto.TourId
                // Removed CreatedAt as per request
            };

            // 3. Save to Database
            _uow.Repository<Review>().Add(review);
            await _uow.CompleteAsync();

            // 4. Fetch details for return
            var user = await _uow.Repository<User>().GetByIdAsync(dto.UserId);
            string targetName = "Unknown";

            if (dto.ListingId.HasValue)
            {
                var listing = await _uow.Repository<Listing>().GetByIdAsync(dto.ListingId.Value);
                if (listing != null) targetName = listing.Title;
            }
            else if (dto.TourId.HasValue)
            {
                var tour = await _uow.Repository<Tour>().GetByIdAsync(dto.TourId.Value);
                if (tour != null) targetName = tour.Title;
            }

            return new ReviewDTO
            {
                ReviewId = review.ReviewId,
                Rating = review.Rating,
                Comment = review.Comment,
                GuestName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown Guest",
                TargetName = targetName
            };
        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsByListingIdAsync(int listingId)
        {
            // ★ FIX: Use direct arguments instead of named parameters
            var reviews = await _uow.Repository<Review>().GetAllAsync(
                r => r.ListingId == listingId, // Filter
                r => r.User                    // Include User
            );

            return reviews.Select(r => new ReviewDTO
            {
                ReviewId = r.ReviewId,
                Rating = r.Rating,
                Comment = r.Comment,
                // Handle null User just in case
                GuestName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Anonymous",
                TargetName = "Listing Review"
                // CreatedAt removed
            }).OrderByDescending(r => r.ReviewId).ToList(); // Sort by ID (Newest first)
        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsByTourIdAsync(int tourId)
        {
            // ★ FIX: Use direct arguments
            var reviews = await _uow.Repository<Review>().GetAllAsync(
                r => r.TourId == tourId,       // Filter
                r => r.User                    // Include User
            );

            return reviews.Select(r => new ReviewDTO
            {
                ReviewId = r.ReviewId,
                Rating = r.Rating,
                Comment = r.Comment,
                GuestName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Anonymous",
                TargetName = "Tour Review"
                // CreatedAt removed
            }).OrderByDescending(r => r.ReviewId).ToList(); // Sort by ID (Newest first)
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            var review = await _uow.Repository<Review>().GetByIdAsync(reviewId);
            if (review == null) return false;

            _uow.Repository<Review>().Remove(review);
            await _uow.CompleteAsync();
            return true;
        }
    }
}