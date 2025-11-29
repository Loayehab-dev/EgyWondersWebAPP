using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EgyWonders.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HostDocumentsController : ControllerBase
    {
        private readonly IHostDocumentService _documentService;

        public HostDocumentsController(IHostDocumentService documentService)
        {
            _documentService = documentService;
        }

        // POST: api/HostDocuments
        //  Uses [FromForm] for File Upload
       [Authorize(Roles = "Host, TourGuide, Admin")]
        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] HostDocumentCreateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = await _documentService.UploadDocumentAsync(dto);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var documents = await _documentService.GetDocumentsByUserIdAsync(userId);
            return Ok(documents);
        }
        [Authorize(Roles = "Host, TourGuide, Admin")]
        [HttpDelete("{documentId}")]
        public async Task<IActionResult> Delete(int documentId)
        {
            var deleted = await _documentService.DeleteDocumentAsync(documentId);
            if (!deleted) return NotFound(new { message = "Document not found" });
            return Ok(new { message = "Document deleted successfully" });
        }
    }
}