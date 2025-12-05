using System.Collections.Generic;
using System.Threading.Tasks;
using EgyWonders.DTO;

namespace EgyWonders.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDTO> CreateReviewAsync(ReviewCreateDTO dto);

        Task<IEnumerable<ReviewDTO>> GetReviewsByListingIdAsync(int listingId);

        Task<IEnumerable<ReviewDTO>> GetReviewsByTourIdAsync(int tourId);

        Task<bool> DeleteReviewAsync(int reviewId);
    }
}