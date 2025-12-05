using MVC_front_End_.Models;

namespace MVC_front_End_.Services
{
    public interface IGuestService
    {
        Task<List<ListingDTO>> GetListingsAsync(string search = null, string category = null, decimal? minPrice = null, decimal? maxPrice = null, int? capacity = null);
        Task<ListingDTO> GetListingByIdAsync(int id);
    }
}
