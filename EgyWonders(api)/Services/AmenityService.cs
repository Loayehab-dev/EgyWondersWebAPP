using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EgyWonders.DTO;
using EgyWonders.Models;
using EgyWonders.Interfaces; 

namespace EgyWonders.Services
{
    public class AmenityService : IAmenityService
    {
        private readonly IUnitOfWork _uow;

        public AmenityService(IUnitOfWork uow)
        {
            _uow = uow;
        }


        public async Task<IEnumerable<AmenityDTO>> GetAllAmenitiesAsync()
        {
            // fetch raw data + Listings
            var entities = await _uow.Repository<Amenity>().GetAllAsync(null, x => x.Listings);

           
            var dtos = entities.Select(amenity => new AmenityDTO
            {
                AmenitiesId = amenity.AmenitiesId,
                AmenityName = amenity.AmenityName,

               
                Listings = amenity.Listings.Select(l => new ListingDTO
                {
                    ListingId = l.ListingId,
                    Title = l.Title,
                    PricePerNight = l.PricePerNight,
                    CityName = l.CityName,
                    Status = l.Status
                }).ToList()
            });

            return dtos;
        }


        public async Task<AmenityDTO> GetAmenityByIdAsync(int id)
        {
            var result = await _uow.Repository<Amenity>().GetAllAsync(
                x => x.AmenitiesId == id,
                x => x.Listings
            );

            var entity = result.FirstOrDefault();
            if (entity == null) return null;

            // Map Entity -> DTO
            return new AmenityDTO
            {
                AmenitiesId = entity.AmenitiesId,
                AmenityName = entity.AmenityName,
                Listings = entity.Listings.Select(l => new ListingDTO
                {
                    ListingId = l.ListingId,
                    Title = l.Title,
                    PricePerNight = l.PricePerNight,
                    CityName = l.CityName,
                    Status = l.Status
                }).ToList()
            };
        }

        // 3. CREATE
        public async Task<AmenityDTO> CreateAmenityAsync(AmenityDTO amenityDto)
        {
            var newAmenity = new Amenity
            {
                AmenityName = amenityDto.AmenityName
            };

            _uow.Repository<Amenity>().Add(newAmenity);
            await _uow.CompleteAsync();

            amenityDto.AmenitiesId = newAmenity.AmenitiesId;
            return amenityDto;
        }


        public async Task<bool> DeleteAmenityAsync(int id)
        {
            var result = await _uow.Repository<Amenity>().GetAllAsync(
                filter: x => x.AmenitiesId == id,
                includes: x => x.Listings
            );

            var amenityToDelete = result.FirstOrDefault();

            if (amenityToDelete == null) return false;

            if (amenityToDelete.Listings != null && amenityToDelete.Listings.Any())
            {
                throw new Exception($"Cannot delete '{amenityToDelete.AmenityName}' because it is assigned to {amenityToDelete.Listings.Count} listings.");
            }

            _uow.Repository<Amenity>().Remove(amenityToDelete);
            await _uow.CompleteAsync();

            return true;
        }

        
    }
}