using EgyWonders.DTO;

namespace EgyWonders.Services
{
    public interface IGuideCertificationService
    {
       
        Task<GuideCertificationDto> AddCertificationAsync(GuideCertificationCreateDto dto);

        Task<IEnumerable<GuideCertificationDto>> GetCertificationsByGuideIdAsync(int guideId);

        // Delete operation (removes file from disk)
        Task<bool> DeleteCertificationAsync(int certificationId);
    }
}
