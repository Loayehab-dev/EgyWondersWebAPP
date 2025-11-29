using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EgyWonders.DTO;
using EgyWonders.Models;
using EgyWonders.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EgyWonders.Services
{
    public class TourService : ITourService
    {
        private readonly IUnitOfWork _uow;

        public TourService(IUnitOfWork uow)
        {
            _uow = uow;
        }


        public async Task<IEnumerable<TourDTO>> GetAllToursAsync()
        {
            // Eager load schedules to show available dates
            var tours = await _uow.Repository<Tour>().GetAllAsync(
                null,
                x => x.TourSchedules
            );

            return tours.Select(t => MapToDto(t));
        }

        public async Task<TourDTO> GetTourByIdAsync(int id)
        {
            var tour = (await _uow.Repository<Tour>().GetAllAsync(
                filter: x => x.TourId == id,
                includes: x => x.TourSchedules
            )).FirstOrDefault();

            if (tour == null) return null;

            return MapToDto(tour);
        }

        // Inside Services/TourService.cs

        public async Task<TourDTO> CreateTourAsync(CreateTourDTO dto)
        {
            var tour = new Tour
            {
              
                Title = dto.Title,
                Category = dto.Category,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow,

              
                BasePrice = dto.BasePrice ?? 0, 

              
                Description = dto.Description,
                CityName = dto.CityName,

                // Coordinates (Now correctly mapped)
                CityLatitude = dto.CityLatitude,
                CityLongitude = dto.CityLongitude,

                // If your Tour entity has other required fields (e.g., Status), ensure they are set here.
            };

            _uow.Repository<Tour>().Add(tour);
            await _uow.CompleteAsync();

            return MapToDto(tour);
        }

        public async Task<TourDTO> UpdateTourAsync(int id, CreateTourDTO dto)
        {
            var tour = await _uow.Repository<Tour>().GetByIdAsync(id);
            if (tour == null) throw new Exception("Tour not found.");

            // Update fields
            tour.Title = dto.Title;
            tour.Description = dto.Description;
            tour.Category = dto.Category;
            tour.CityName = dto.CityName;
            tour.CityLatitude = dto.CityLatitude;
            tour.CityLongitude = dto.CityLongitude;
            if (dto.BasePrice.HasValue) tour.BasePrice = dto.BasePrice;

            _uow.Repository<Tour>().Update(tour);
            await _uow.CompleteAsync();

            return await GetTourByIdAsync(id);
        }

        public async Task<bool> DeleteTourAsync(int id)
        {
            // Check dependencies (Bookings)
            var tour = (await _uow.Repository<Tour>().GetAllAsync(
                filter: x => x.TourId == id,
                includes: x => x.TourSchedules
            )).FirstOrDefault();

            if (tour == null) return false;

            // If any schedule has bookings, prevent delete
            if (tour.TourSchedules.Any(s => (s.CurrentBooked ?? 0) > 0))
            {
                throw new Exception("Cannot delete Tour: It has active bookings.");
            }

            _uow.Repository<Tour>().Remove(tour);
            await _uow.CompleteAsync();
            return true;
        }

       
        //   SCHEDULE MANAGEMENT (Merged)
        

        public async Task<TourScheduleDTO> AddScheduleAsync(TourScheduleCreateDTO dto)
        {
            if (dto.StartTime < DateTime.UtcNow)
                throw new Exception("Cannot create a schedule in the past.");

            if (dto.EndTime <= dto.StartTime)
                throw new Exception("End time must be after start time.");

            var schedule = new TourSchedule
            {
                TourId = dto.TourId,
                // Extract DateOnly from the DateTime provided
                Date = DateOnly.FromDateTime(dto.StartTime),
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                MaxParticipants = dto.MaxParticipants,
                CurrentBooked = 0
            };

            _uow.Repository<TourSchedule>().Add(schedule);
            await _uow.CompleteAsync();

            return new TourScheduleDTO
            {
                ScheduleId = schedule.ScheduleId,
                Date = schedule.Date,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                MaxParticipants = schedule.MaxParticipants,
                CurrentBooked = 0
            };
        }

        public async Task<bool> DeleteScheduleAsync(int scheduleId)
        {
            var schedule = (await _uow.Repository<TourSchedule>().GetAllAsync(
                filter: s => s.ScheduleId == scheduleId,
                includes: x => x.TourBookings
            )).FirstOrDefault();

            if (schedule == null) return false;

            // Check if anyone booked this specific slot
            if ((schedule.CurrentBooked ?? 0) > 0 || schedule.TourBookings.Any(b => b.Status != "Cancelled"))
            {
                throw new Exception("Cannot delete schedule: Active bookings exist.");
            }

            _uow.Repository<TourSchedule>().Remove(schedule);
            await _uow.CompleteAsync();
            return true;
        }

        // HELPER
        private TourDTO MapToDto(Tour tour)
        {
            return new TourDTO
            {
                TourId = tour.TourId,
                Title = tour.Title,
                Description = tour.Description,
                BasePrice = tour.BasePrice ?? 0,
                CityName = tour.CityName,
                Schedules = tour.TourSchedules?.Select(s => new TourScheduleDTO
                {
                    ScheduleId = s.ScheduleId,
                    Date = s.Date,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MaxParticipants = s.MaxParticipants,
                    CurrentBooked = s.CurrentBooked ?? 0
                }).ToList() ?? new List<TourScheduleDTO>()
            };
        }
    }
}