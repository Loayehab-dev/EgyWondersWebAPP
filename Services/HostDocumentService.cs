using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // For file paths
using EgyWonders.DTO;
using EgyWonders.Models;
using EgyWonders.Interfaces;

namespace EgyWonders.Services
{
    public class HostDocumentService : IHostDocumentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Inject the Environment to find the folder
        public HostDocumentService(IUnitOfWork uow, IWebHostEnvironment webHostEnvironment)
        {
            _uow = uow;
            _webHostEnvironment = webHostEnvironment;
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
    }
}