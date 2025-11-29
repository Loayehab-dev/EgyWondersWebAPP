using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EgyWonders.DTO;
using EgyWonders.Models;
using EgyWonders.Interfaces;
using Microsoft.EntityFrameworkCore; // Needed for Includes

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
            // 1. Validation: Ensure we are reviewing exactly ONE thing
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
            };

            // 3. Save to Database
            _uow.Repository<Review>().Add(review);
            await _uow.CompleteAsync();

            // 4. Fetch details to return a nice DTO
            // We need the User's name and the Product's title
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
            var reviews = await _uow.Repository<Review>().GetAllAsync(
                filter: r => r.ListingId == listingId,
                includes: x => x.User // Include user to get names
            );

            return reviews.Select(r => new ReviewDTO
            {
                ReviewId = r.ReviewId,
                Rating = r.Rating,
                Comment = r.Comment,
                GuestName = $"{r.User.FirstName} {r.User.LastName}",
                TargetName = "Listing Review"
            }).ToList();
        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsByTourIdAsync(int tourId)
        {
            var reviews = await _uow.Repository<Review>().GetAllAsync(
                filter: r => r.TourId == tourId,
                includes: x => x.User
            );

            return reviews.Select(r => new ReviewDTO
            {
                ReviewId = r.ReviewId,
                Rating = r.Rating,
                Comment = r.Comment,
                GuestName = $"{r.User.FirstName} {r.User.LastName}",
                TargetName = "Tour Review"
            }).ToList();
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