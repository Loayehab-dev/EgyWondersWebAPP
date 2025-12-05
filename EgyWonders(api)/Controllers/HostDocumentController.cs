using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Models;
using EgyWonders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly IAccountService _accontservice;// 1. Inject UserManager

        public HostDocumentsController(IHostDocumentService documentService,IAccountService accountService)
        {
            _documentService = documentService;
            _accontservice = accountService;
        }

        // POST: api/HostDocuments
        //  Uses [FromForm] for File Upload
      
        [HttpPost]
        
public async Task<IActionResult> Upload([FromForm] HostDocumentCreateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // 1. Upload the Document
                var created = await _documentService.UploadDocumentAsync(dto);

                // 2. Promote the User (Using your AccountService)
                await _accontservice.PromoteUserToHostAsync(dto.UserId);

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

        [Authorize(Roles = "Admin")] // ONLY admins can access this endpoint
        [HttpPost("approve/{documentId}")]
        public async Task<IActionResult> ApproveDocument(int documentId)
        {
            var success = await _documentService.ApproveDocumentAsync(documentId);

            if (success)
            {
                return Ok(new { message = "Document approved and user promoted to Host." });
            }

            // Return 404 if the document was not found or already processed.
            return NotFound(new { message = "Failed to approve document. It may not exist or cannot be verified." });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var documents = await _documentService.GetPendingDocumentsAsync();
            return Ok(documents); // This sends the list of documents to the MVC app
        }

    }
}