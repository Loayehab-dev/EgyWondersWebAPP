using EgyWonders.DTO;

namespace EgyWonders.Services
{
    public interface IHostDocumentService
    {
        Task<HostDocumentDTO> UploadDocumentAsync(HostDocumentCreateDTO dto);

        
        Task<IEnumerable<HostDocumentDTO>> GetDocumentsByUserIdAsync(int userId);

       
        Task<bool> DeleteDocumentAsync(int documentId);
    }
}
