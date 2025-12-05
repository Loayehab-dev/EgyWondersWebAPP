using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Models;
using Microsoft.AspNetCore.Hosting; // For file paths
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EgyWonders.Services
{
    public class HostDocumentService : IHostDocumentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager; // 1. ADDED: For role management

        // Updated Constructor
        public HostDocumentService(IUnitOfWork uow, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager; // Inject UserManager
        }
        public async Task<HostDocumentDTO> UploadDocumentAsync(HostDocumentCreateDTO dto)
        {
            string documentUrl = "";

            // 📂 HANDLE FILE UPLOAD 📂
            if (dto.Document != null && dto.Document.Length > 0)
            {
                // 1. Prepare folder: wwwroot/documents
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "documents");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // 2. Generate unique filename
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Document.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 3. Save to disk
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Document.CopyToAsync(fileStream);
                }

                // 4. Set the relative path for the DB
                documentUrl = $"/documents/{uniqueFileName}";
            }
            else
            {
                throw new Exception("File is empty or missing.");
            }

            // Map DTO to Entity
            var document = new HostDocument
            {
                UserId = dto.UserId,
                DocumentPath = documentUrl, // Save the path we just created
                NationalId = dto.NationalId,
                TextRecord = dto.TextRecord,
                Verified = false,
                CreatedAt = DateTime.UtcNow
            };

            _uow.Repository<HostDocument>().Add(document);
            await _uow.CompleteAsync();

            return new HostDocumentDTO
            {
                DocumentId = document.DocumentId,
                UserId = document.UserId,
                DocumentPath = document.DocumentPath,
                NationalId = document.NationalId,
                TextRecord = document.TextRecord,
                Verified = document.Verified,
                CreatedAt = document.CreatedAt
            };
        }

        // ... Get and Delete methods remain exactly the same as before ...
        public async Task<IEnumerable<HostDocumentDTO>> GetDocumentsByUserIdAsync(int userId)
        {
            var documents = await _uow.Repository<HostDocument>().GetAllAsync(d => d.UserId == userId);
            return documents.Select(d => new HostDocumentDTO
            {
                DocumentId = d.DocumentId,
                UserId = d.UserId,
                DocumentPath = d.DocumentPath,
                NationalId = d.NationalId,
                TextRecord = d.TextRecord,
                Verified = d.Verified,
                CreatedAt = d.CreatedAt
            }).ToList();
        }

        public async Task<bool> DeleteDocumentAsync(int documentId)
        {
            var document = await _uow.Repository<HostDocument>().GetByIdAsync(documentId);
            if (document == null) return false;

            // Optional: Delete file from disk
            if (!string.IsNullOrEmpty(document.DocumentPath))
            {
                var fileName = Path.GetFileName(document.DocumentPath);
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "documents", fileName);
                if (File.Exists(path)) File.Delete(path);
            }

            _uow.Repository<HostDocument>().Remove(document);
            await _uow.CompleteAsync();
            return true;
        }
        public async Task<bool> ApproveDocumentAsync(int documentId)
        {
            // 1. FETCH DOCUMENT: Get the document using its primary key (DocumentId)
            // We assume GetByIdAsync works correctly without includes.
            var document = await _uow.Repository<HostDocument>().GetByIdAsync(documentId);

            if (document == null || document.Verified) return false;

            // 2. FETCH USER: Use the foreign key (UserId) to get the necessary data.
            // This bypasses the faulty 'includeProperties' string.
            var businessUser = await _uow.Repository<User>().GetByIdAsync(document.UserId);

            // CRITICAL CHECKS: Ensure the link exists and has the Identity ID
            if (businessUser == null || string.IsNullOrEmpty(businessUser.AspNetUserId))
            {
                // The document is orphaned or linked incorrectly.
                return false;
            }

            // 3. FIND IDENTITY USER: Get the user from the Identity system (UserManager)
            var aspNetUser = await _userManager.FindByIdAsync(businessUser.AspNetUserId);
            if (aspNetUser == null) return false;

            // 4. ASSIGN ROLE (Business Logic)
            if (!await _userManager.IsInRoleAsync(aspNetUser, "Host"))
            {
                var roleResult = await _userManager.AddToRoleAsync(aspNetUser, "Host");
                if (!roleResult.Succeeded) return false;
            }

            // 5. UPDATE & SAVE
            document.Verified = true;
            _uow.Repository<HostDocument>().Update(document);
            await _uow.CompleteAsync();

            return true;
        }
        public async Task<IEnumerable<HostDocumentDTO>> GetPendingDocumentsAsync()
        {
            // 1. Get unverified documents without broken includes
            var documents = await _uow.Repository<HostDocument>()
                .GetAllAsync(
                    // Filter is sufficient: look for non-verified documents
                    filter: d => d.Verified == false
                );

            if (documents == null) return new List<HostDocumentDTO>();

            // 2. Map and fetch the User details individually (Safest method)
            var dtoList = new List<HostDocumentDTO>();
            var userRepository = _uow.Repository<User>();

            foreach (var d in documents)
            {
                // Fetch the user by the foreign key (UserId) to get their first/last name
                var user = await userRepository.GetByIdAsync(d.UserId);

                dtoList.Add(new HostDocumentDTO
                {
                    DocumentId = d.DocumentId,
                    UserId = d.UserId,
                    NationalId = d.NationalId,
                    DocumentPath = d.DocumentPath,
                    CreatedAt = d.CreatedAt,

                    // FIX: We will return the name using a temporary property in the next step
                    // For now, only map the properties that exist on the DTO.
                });
            }

            return dtoList;
        }
    }
}