using System.Collections.Generic;
using System.Threading.Tasks;
using EgyWonders.DTO;
using EgyWonders.Models;

namespace EgyWonders.Services
{
    public interface IAmenityService
    {
        Task<IEnumerable<AmenityDTO>> GetAllAmenitiesAsync();
        Task<AmenityDTO> GetAmenityByIdAsync(int id);
        Task<AmenityDTO> CreateAmenityAsync(AmenityDTO amenityDto);
        Task<bool> DeleteAmenityAsync(int id);
    }
}
