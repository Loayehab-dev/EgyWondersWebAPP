using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // Required for IWebHostEnvironment
using EgyWonders.DTO;
using EgyWonders.Models;
using EgyWonders.Interfaces;

namespace EgyWonders.Services
{
    public class GuideCertificationService : IGuideCertificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Dependency Injection of UnitOfWork and WebHostEnvironment
        public GuideCertificationService(IUnitOfWork uow, IWebHostEnvironment webHostEnvironment)
        {
            _uow = uow;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================================
        // 1. ADD CERTIFICATION (With File Upload)
        // ==========================================================
        public async Task<GuideCertificationDto> AddCertificationAsync(GuideCertificationCreateDto dto)
        {
            // A. Validation: Expiry must be after Issue date
            if (dto.ExpiryDate <= dto.IssueDate)
            {
                throw new Exception("Expiry Date must be later than the Issue Date.");
            }

            // B.  HANDLE FILE UPLOAD 
            string documentUrl = "";

            if (dto.Document != null && dto.Document.Length > 0)
            {
                // 1. Prepare folder path: wwwroot/certifications
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "certifications");

                // 2. Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // 3. Generate unique filename (GUID + Original Extension)
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Document.FileName);

                // 4. Combine to get full physical path
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 5. Save the file stream to the disk
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Document.CopyToAsync(fileStream);
                }

                // 6. Store the relative path (e.g. /certifications/abc.pdf)
                documentUrl = $"/certifications/{uniqueFileName}";
            }
            else
            {
                throw new Exception("A document file is required.");
            }

            // C. Map DTO to Entity
            var certification = new GuideCertification
            {
                GuideId = dto.GuideId,
                CertificationName = dto.CertificationName,
                IssueDate = dto.IssueDate,
                ExpiryDate = dto.ExpiryDate,
                // Assign the URL string we just created
                DocumentPath = documentUrl
            };

            // D. Save to Database
            _uow.Repository<GuideCertification>().Add(certification);
            await _uow.CompleteAsync();

            // E. Return the Result DTO
            return new GuideCertificationDto
            {
                CertificationId = certification.CertificationId,
                GuideId = certification.GuideId,
                CertificationName = certification.CertificationName,
                IssueDate = certification.IssueDate,
                ExpiryDate = certification.ExpiryDate,
                DocumentPath = certification.DocumentPath
            };
        }

        // ==========================================================
        // 2. GET CERTIFICATIONS
        // ==========================================================
        public async Task<IEnumerable<GuideCertificationDto>> GetCertificationsByGuideIdAsync(int guideId)
        {
            // Fetch certifications filtered by the Guide's User ID
            var certifications = await _uow.Repository<GuideCertification>()
                .GetAllAsync(c => c.GuideId == guideId);

            // Map Entities to DTOs
            return certifications.Select(c => new GuideCertificationDto
            {
                CertificationId = c.CertificationId,
                GuideId = c.GuideId,
                CertificationName = c.CertificationName,
                IssueDate = c.IssueDate,
                ExpiryDate = c.ExpiryDate,
                DocumentPath = c.DocumentPath
            }).ToList();
        }

        // ==========================================================
        // 3. DELETE CERTIFICATION (And Physical File)
        // ==========================================================
        public async Task<bool> DeleteCertificationAsync(int certificationId)
        {
            var certification = await _uow.Repository<GuideCertification>().GetByIdAsync(certificationId);

            if (certification == null) return false;

            // ⚠️ Cleanup: Delete physical file from disk to save space
            if (!string.IsNullOrEmpty(certification.DocumentPath))
            {
                var fileName = Path.GetFileName(certification.DocumentPath);
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "certifications", fileName);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            // Remove record from DB
            _uow.Repository<GuideCertification>().Remove(certification);
            await _uow.CompleteAsync();

            return true;
        }
    }
}