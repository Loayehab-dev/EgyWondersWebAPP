using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EgyWonders.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToursController : ControllerBase
    {
        private readonly ITourService _tourService;

        public ToursController(ITourService tourService)
        {
            _tourService = tourService;
        }

      

        // GET: api/Tours
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tours = await _tourService.GetAllToursAsync();
            return Ok(tours);
        }

        // GET: api/Tours/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var tour = await _tourService.GetTourByIdAsync(id);
            if (tour == null) return NotFound(new { message = "Tour not found" });
            return Ok(tour);
        }

        // POST: api/Tours (Create a new tour product)
        [HttpPost]
        public async Task<IActionResult> CreateTour(CreateTourDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _tourService.CreateTourAsync(dto);
            return Ok(created);
        }

        // PUT: api/Tours/5 (Update tour details)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTour(int id, CreateTourDTO dto)
        {
            try
            {
                var updated = await _tourService.UpdateTourAsync(id, dto);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                // This catches the 'Tour not found' error from the service
                return NotFound(new { error = ex.Message });
            }
        }

        // DELETE: api/Tours/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            try
            {
                var deleted = await _tourService.DeleteTourAsync(id);
                if (!deleted) return NotFound(new { message = "Tour not found" });
                return Ok(new { message = "Tour deleted successfully" });
            }
            catch (Exception ex)
            {
                // Catches the 'Active bookings exist' error
                return BadRequest(new { error = ex.Message });
            }
        }


        // POST: api/Tours/schedule (Add a new time slot to a tour)
        [HttpPost("schedule")]
        public async Task<IActionResult> AddSchedule(TourScheduleCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var created = await _tourService.AddScheduleAsync(dto);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        // DELETE: api/Tours/schedule/201 (Delete a specific time slot)
        [HttpDelete("schedule/{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            try
            {
                var deleted = await _tourService.DeleteScheduleAsync(id);
                if (!deleted) return NotFound(new { message = "Schedule not found" });
                return Ok(new { message = "Schedule deleted" });
            }
            catch (Exception ex)
            {
                
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}