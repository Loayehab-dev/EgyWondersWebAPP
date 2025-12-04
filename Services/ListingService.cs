using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EgyWonders.DTO;
using EgyWonders.Models;
using EgyWonders.Interfaces;

namespace EgyWonders.Services
{
    public class ListingService : IListingService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ListingService(IUnitOfWork uow, IWebHostEnvironment webHostEnvironment)
        {
            _uow = uow;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IEnumerable<ListingDTO>> GetAllListingsAsync()
        {
            var listings = await _uow.Repository<Listing>().GetAllAsync(
                null,
                x => x.ListingPhotos,
                x => x.Amenities
            );

            return listings.Select(l => new ListingDTO
            {
                ListingId = l.ListingId,
                Title = l.Title,
                PricePerNight = l.PricePerNight,
                CityName = l.CityName,
                Status = l.Status,
                Category = l.Category,
                UserId = l.UserId,
                Capacity = l.Capacity,
                AmenityNames = l.Amenities.Select(a => a.AmenityName).ToList(),
                // Show only 1st photo as thumbnail
            
                Photos = l.ListingPhotos.Take(1).Select(p => new ListingPhotoDTO
                {
                    PhotoId = p.PhotoId,
                    Url = p.Url,
                    Caption = p.Caption
                }).ToList()
            });
        }

        
        public async Task<ListingDTO> GetListingByIdAsync(int id)
        {
            var result = await _uow.Repository<Listing>().GetAllAsync(
                x => x.ListingId == id,
                x => x.ListingPhotos,
                x => x.Amenities
            );

            var entity = result.FirstOrDefault();
            if (entity == null) return null;

            return new ListingDTO
            {
                ListingId = entity.ListingId,
                Title = entity.Title,
                PricePerNight = entity.PricePerNight,
                CityName = entity.CityName,
                Status = entity.Status,
                Category = entity.Category,
                UserId = entity.UserId,
                Capacity = entity.Capacity,
                AmenityNames = entity.Amenities.Select(a => a.AmenityName).ToList(),
                Photos = entity.ListingPhotos.Select(p => new ListingPhotoDTO
                {
                    PhotoId = p.PhotoId,
                    Url = p.Url,
                    Caption = p.Caption
                }).ToList()
            };
        }

       
        public async Task<ListingDTO> CreateListingAsync(ListingCreateDTO dto)
        {
            var listing = new Listing
            {
                Title = dto.Title,
                PricePerNight = dto.PricePerNight,
                CityName = dto.CityName,
                CityLongitude = dto.CityLongitude,
                CityLatitude = dto.CityLatitude,
                Status = "Pending",
                Category = dto.Category,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow,
                Capacity = dto.Capacity > 0 ? dto.Capacity : 1
            };

            // A. Handle Photos
            await ProcessPhotos(dto.Photos, listing);

            // B. Handle Amenities
            if (dto.AmenityIds != null && dto.AmenityIds.Any())
            {
                var amenities = await _uow.Repository<Amenity>()
                    .GetAllAsync(a => dto.AmenityIds.Contains(a.AmenitiesId));

                foreach (var am in amenities) listing.Amenities.Add(am);
            }

            _uow.Repository<Listing>().Add(listing);
            await _uow.CompleteAsync();

            return await GetListingByIdAsync(listing.ListingId);
        }

       
        public async Task<ListingDTO> UpdateListingAsync(int id, ListingUpdateDTO dto)
        {
            // Load Listing with relations so we can update them
            var result = await _uow.Repository<Listing>().GetAllAsync(
                x => x.ListingId == id,
                x => x.Amenities,
                x => x.ListingPhotos
            );

            var listing = result.FirstOrDefault();
            if (listing == null) throw new Exception("Listing not found");

            // A. Update Text Fields (Only if provided)
            if (dto.Title != null) listing.Title = dto.Title;
            if (dto.PricePerNight.HasValue) listing.PricePerNight = dto.PricePerNight.Value;
            if (dto.CityName != null) listing.CityName = dto.CityName;
            if (dto.Category != null) listing.Category = dto.Category;
            if (dto.Status != null) listing.Status = dto.Status;
            if (dto.Capacity.HasValue) listing.Capacity = dto.Capacity.Value;

            // B. Add NEW Photos
            await ProcessPhotos(dto.NewPhotos, listing);

            // C.  (Replace old list with new list)
            if (dto.AmenityIds != null)
            {
                // Clear current links
                listing.Amenities.Clear();

                // Add new links
                if (dto.AmenityIds.Any())
                {
                    var newAmenities = await _uow.Repository<Amenity>()
                        .GetAllAsync(a => dto.AmenityIds.Contains(a.AmenitiesId));

                    foreach (var am in newAmenities) listing.Amenities.Add(am);
                }
            }

            _uow.Repository<Listing>().Update(listing);
            await _uow.CompleteAsync();

            return await GetListingByIdAsync(id);
        }

      
        public async Task<bool> DeleteListingAsync(int id)
        {
            // We need Bookings (to check safety) and Photos (to delete files)
            var result = await _uow.Repository<Listing>().GetAllAsync(
                filter: x => x.ListingId == id,
                includes: new System.Linq.Expressions.Expression<Func<Listing, object>>[]
                {
                    x => x.ListingBookings,
                    x => x.ListingPhotos
                }
            );

            var listing = result.FirstOrDefault();
            if (listing == null) return false;

            // A. Safety Check
            if (listing.ListingBookings.Any())
            {
                throw new Exception("Cannot delete Listing because it has active bookings.");
            }

            // B. Delete Files from Disk
            if (listing.ListingPhotos != null)
            {
                foreach (var photo in listing.ListingPhotos)
                {
                    // URL is like "/uploads/abc.jpg". We need physical path.
                    if (!string.IsNullOrEmpty(photo.Url))
                    {
                        var fileName = Path.GetFileName(photo.Url);
                        var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);
                        if (File.Exists(physicalPath))
                        {
                            File.Delete(physicalPath);
                        }
                    }
                }
            }

            // C. Delete from DB
            _uow.Repository<Listing>().Remove(listing);
            await _uow.CompleteAsync();
            return true;
        }

        //helper method to process photo uploads
        private async Task ProcessPhotos(List<IFormFile> files, Listing listing)
        {
            if (files != null && files.Any())
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        listing.ListingPhotos.Add(new ListingPhoto
                        {
                            Url = $"/uploads/{uniqueFileName}",
                            Caption = file.FileName
                        });
                    }
                }
            }
        }
        public async Task<bool> DeletePhotoAsync(int photoId)
        {
            // 1. Find the photo
            var photo = await _uow.Repository<ListingPhoto>().GetByIdAsync(photoId);
            if (photo == null) return false;

            // 2. Delete the physical file from "wwwroot/uploads"
            if (!string.IsNullOrEmpty(photo.Url))
            {
                // Convert "/uploads/abc.jpg" -> "C:\...\wwwroot\uploads\abc.jpg"
                var fileName = Path.GetFileName(photo.Url);
                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }

            // 3. Remove from Database
            _uow.Repository<ListingPhoto>().Remove(photo);
            await _uow.CompleteAsync();

            return true;
        }
        public async Task<bool> UpdateListingStatusAsync(int id, string newStatus)
        {
            // 1. Get the listing from the repo
            var listing = await _uow.Repository<Listing>().GetByIdAsync(id);

            // 2. If it doesn't exist, return false
            if (listing == null) return false;

            // 3. Update the status
            listing.Status = newStatus;

            // 4. Update and Save changes
            _uow.Repository<Listing>().Update(listing);
            await _uow.CompleteAsync();

            return true;
        }
    }
}