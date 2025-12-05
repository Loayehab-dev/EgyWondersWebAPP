using MVC_front_End_.Models;
using System.Threading.Tasks;

namespace MVC_front_End_.Services
{
    public interface IHostService
    {
        Task<HostDashboardViewModel> GetDashboardStatsAsync();
       Task<List<AmenityDTO>> GetAmenitiesAsync();
        Task<bool> CreateListingAsync(ListingCreateDTO model);
        Task<bool> UpdateListingAsync(int id, ListingUpdateDTO model);
        Task<ListingDTO> GetListingByIdAsync(int id);
        Task<bool> DeleteListingAsync(int id);
        Task<List<ListingDTO>> GetHostListingsByIdAsync(int hostId);
    }
}