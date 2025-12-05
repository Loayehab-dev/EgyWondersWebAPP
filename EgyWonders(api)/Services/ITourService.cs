using EgyWonders.DTO;

namespace EgyWonders.Services
{
    public interface ITourService
    {
        Task<IEnumerable<TourDTO>> GetAllToursAsync();
        Task<TourDTO> GetTourByIdAsync(int id);
        Task<TourDTO> CreateTourAsync(CreateTourDTO dto);
        Task<TourDTO> UpdateTourAsync(int id, CreateTourDTO dto);
        Task<bool> DeleteTourAsync(int id);

        Task<TourScheduleDTO> AddScheduleAsync(TourScheduleCreateDTO dto);
        Task<bool> DeleteScheduleAsync(int scheduleId);
    }
}
