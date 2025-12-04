using EgyWonders.DTO;

namespace EgyWonders.Services
{
    public interface IHostDashbordService
    {
        
            Task<HostStatsDTO> GetHostStatsAsync(int hostUserId);
        
    }
}
