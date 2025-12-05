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
    public class GuideCertificationsController : ControllerBase
    {
        private readonly IGuideCertificationService _certificationService;

        public GuideCertificationsController(IGuideCertificationService certificationService)
        {
            _certificationService = certificationService;
        }

        // POST: api/GuideCertifications
        [Authorize(Roles = "Host, TourGuide, Admin")]
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] GuideCertificationCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = await _certificationService.AddCertificationAsync(dto);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/GuideCertifications/guide/101
        [HttpGet("guide/{guideId}")]
        public async Task<IActionResult> GetByGuideId(int guideId)
        {
            var certifications = await _certificationService.GetCertificationsByGuideIdAsync(guideId);
            return Ok(certifications);
        }

        // DELETE: api/GuideCertifications/5
        [Authorize(Roles = "Host, TourGuide, Admin")]
        [HttpDelete("{certificationId}")]
        public async Task<IActionResult> Delete(int certificationId)
        {
            var deleted = await _certificationService.DeleteCertificationAsync(certificationId);
            if (!deleted) return NotFound(new { message = "Certification not found" });
            return Ok(new { message = "Certification deleted successfully" });
        }
    }
}