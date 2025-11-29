using EgyWonders.DTO;

namespace EgyWonders.Services
{
    public interface IListingService
    {
        Task<IEnumerable<ListingDTO>> GetAllListingsAsync();
        Task<ListingDTO> GetListingByIdAsync(int id);
        Task<ListingDTO> CreateListingAsync(ListingCreateDTO dto);
        Task<ListingDTO> UpdateListingAsync(int id, ListingUpdateDTO dto);
        Task<bool> DeleteListingAsync(int id);
        Task<bool> DeletePhotoAsync(int photoId);
    }
}
