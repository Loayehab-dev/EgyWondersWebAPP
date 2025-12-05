using EgyWonders.DTO;
using EgyWonders.Interfaces;
using EgyWonders.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions; // Required for Expression<Func<...>>
using System.Threading.Tasks;

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
                x => x.Amenities,
                x => x.Reviews // Include Reviews for Rating Calculation
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

                // ★ DYNAMIC RATING LOGIC ★
                AverageRating = (l.Reviews != null && l.Reviews.Any()) ? l.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = l.Reviews?.Count ?? 0,

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
                x => x.Amenities,
                x => x.Reviews // Include Reviews
            );

            var entity = result.FirstOrDefault();
            if (entity == null) return null;

            // Calculate Rating Logic
            double avgRating = 0;
            if (entity.Reviews != null && entity.Reviews.Any())
            {
                avgRating = entity.Reviews.Average(r => r.Rating);
            }

            return new ListingDTO
            {
                ListingId = entity.ListingId,
                Title = entity.Title,
                Description = entity.Description, // Ensure description is mapped
                PricePerNight = entity.PricePerNight,
                CityName = entity.CityName,
                Status = entity.Status,
                Category = entity.Category,
                UserId = entity.UserId,
                Capacity = entity.Capacity,
                AmenityNames = entity.Amenities.Select(a => a.AmenityName).ToList(),

                // Map Ratings
                AverageRating = avgRating,
                ReviewCount = entity.Reviews?.Count ?? 0,

                // Map Photos
                Photos = entity.ListingPhotos.Select(p => new ListingPhotoDTO
                {
                    PhotoId = p.PhotoId,
                    Url = p.Url,
                    Caption = p.Caption
                }).ToList(),

                // Map Reviews
                Reviews = entity.Reviews?.Select(r => new ReviewDTO
                {
                    ReviewId = r.ReviewId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    // Handle potential null User
                    GuestName = r.User != null ? r.User.FirstName : "Guest"
                }).OrderByDescending(r => r.ReviewId).ToList() ?? new List<ReviewDTO>()
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
            var result = await _uow.Repository<Listing>().GetAllAsync(
                x => x.ListingId == id,
                x => x.Amenities,
                x => x.ListingPhotos
            );

            var listing = result.FirstOrDefault();
            if (listing == null) throw new Exception("Listing not found");

            // A. Update Text Fields
            if (dto.Title != null) listing.Title = dto.Title;
            if (dto.PricePerNight.HasValue) listing.PricePerNight = dto.PricePerNight.Value;
            if (dto.CityName != null) listing.CityName = dto.CityName;
            if (dto.Category != null) listing.Category = dto.Category;
            if (dto.Status != null) listing.Status = dto.Status;
            if (dto.Capacity.HasValue) listing.Capacity = dto.Capacity.Value;

            // B. Add NEW Photos
            await ProcessPhotos(dto.NewPhotos, listing);

            // C. Update Amenities
            if (dto.AmenityIds != null)
            {
                listing.Amenities.Clear();
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
            var result = await _uow.Repository<Listing>().GetAllAsync(
                filter: x => x.ListingId == id,
                includes: new Expression<Func<Listing, object>>[]
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

        public async Task<bool> DeletePhotoAsync(int photoId)
        {
            var photo = await _uow.Repository<ListingPhoto>().GetByIdAsync(photoId);
            if (photo == null) return false;

            if (!string.IsNullOrEmpty(photo.Url))
            {
                var fileName = Path.GetFileName(photo.Url);
                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }

            _uow.Repository<ListingPhoto>().Remove(photo);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<bool> UpdateListingStatusAsync(int id, string newStatus)
        {
            var listing = await _uow.Repository<Listing>().GetByIdAsync(id);
            if (listing == null) return false;

            listing.Status = newStatus;
            _uow.Repository<Listing>().Update(listing);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<IEnumerable<ListingDTO>> GetListingsByUserIdAsync(int userId)
        {
            var listings = await _uow.Repository<Listing>().GetAllAsync(
                l => l.UserId == userId,
                l => l.ListingPhotos,
                l => l.Amenities,
                l => l.Reviews
            );

            return listings.Select(l => new ListingDTO
            {
                ListingId = l.ListingId,
                Title = l.Title,
                PricePerNight = l.PricePerNight,
                CityName = l.CityName,
                Status = l.Status,
                Category = l.Category,
                Capacity = l.Capacity,
                UserId = l.UserId,

                AverageRating = (l.Reviews != null && l.Reviews.Any()) ? l.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = l.Reviews?.Count ?? 0,

                Photos = l.ListingPhotos?.Select(p => new ListingPhotoDTO
                {
                    PhotoId = p.PhotoId,
                    Url = p.Url
                }).ToList() ?? new List<ListingPhotoDTO>()
            });
        }

        // Helper Method
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
    }
}
