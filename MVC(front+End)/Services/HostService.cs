using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MVC_front_End_.Models;
using System.Collections.Generic; // Required for List<>
using System;

namespace MVC_front_End_.Services
{
    public class HostService : IHostService
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContext;

        public HostService(HttpClient client, IHttpContextAccessor httpContext)
        {
            _client = client;
            _httpContext = httpContext;
        }

        // Helper to add Token automatically
        private void AddAuthorization()
        {
            var token = _httpContext.HttpContext?.User.FindFirst("JWT")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // 1. Get Dashboard Statistics
        public async Task<HostDashboardViewModel> GetDashboardStatsAsync()
        {
            AddAuthorization();
            try
            {
                var response = await _client.GetAsync("api/HostDashboard/stats");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HostDashboardViewModel>();
                }
            }
            catch { /* Ignore errors */ }
            return new HostDashboardViewModel();
        }

        // 2. Get Listings by Host ID (Fixed Method)
        public async Task<List<ListingDTO>> GetHostListingsByIdAsync(int hostId)
        {
            AddAuthorization();
            var response = await _client.GetAsync($"api/Listings/host/{hostId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ListingDTO>>();
            }
            return new List<ListingDTO>();
        }

        // 3. Get Single Listing (For Editing)
        public async Task<ListingDTO> GetListingByIdAsync(int id)
        {
            var response = await _client.GetAsync($"api/Listings/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ListingDTO>();
            }
            return null;
        }

        // 4. Get Amenities List (For Checkboxes)
        public async Task<List<AmenityDTO>> GetAmenitiesAsync()
        {
            try
            {
                var response = await _client.GetAsync("api/Amenities");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<AmenityDTO>>();
                }
            }
            catch { }
            return new List<AmenityDTO>();
        }

        // 5. Create Listing (POST)
        public async Task<bool> CreateListingAsync(ListingCreateDTO model)
        {
            AddAuthorization();

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(model.Title ?? ""), "Title");
                // content.Add(new StringContent(model.Description ?? ""), "Description"); // Uncomment if description is back
                content.Add(new StringContent(model.PricePerNight.ToString()), "PricePerNight");
                content.Add(new StringContent(model.CityName ?? ""), "CityName");
                content.Add(new StringContent(model.Category ?? ""), "Category");
                content.Add(new StringContent(model.Capacity.ToString()), "Capacity");

                content.Add(new StringContent(model.CityLongitude.ToString()), "CityLongitude");
                content.Add(new StringContent(model.CityLatitude.ToString()), "CityLatitude");

                // Add User ID
                var businessId = _httpContext.HttpContext?.User.FindFirst("BusinessId")?.Value;
                if (!string.IsNullOrEmpty(businessId))
                {
                    content.Add(new StringContent(businessId), "UserId");
                }
                else
                {
                    content.Add(new StringContent(model.UserId.ToString()), "UserId");
                }

                // Add Amenities
                if (model.AmenityIds != null)
                {
                    foreach (var id in model.AmenityIds)
                        content.Add(new StringContent(id.ToString()), "AmenityIds");
                }

                // Add Photos
                if (model.Photos != null)
                {
                    foreach (var file in model.Photos)
                    {
                        var fileContent = new StreamContent(file.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        content.Add(fileContent, "Photos", file.FileName);
                    }
                }

                try
                {
                    var response = await _client.PostAsync("api/Listings", content);
                    return response.IsSuccessStatusCode;
                }
                catch { return false; }
            }
        }

        // 6. Update Listing (PUT)
        public async Task<bool> UpdateListingAsync(int id, ListingUpdateDTO model)
        {
            AddAuthorization();

            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(model.Title ?? ""), "Title");
                content.Add(new StringContent(model.CityName ?? ""), "CityName");
                content.Add(new StringContent(model.Category ?? ""), "Category");

                if (model.PricePerNight.HasValue)
                    content.Add(new StringContent(model.PricePerNight.Value.ToString()), "PricePerNight");

                if (model.Capacity.HasValue)
                    content.Add(new StringContent(model.Capacity.Value.ToString()), "Capacity");

                content.Add(new StringContent(model.CityLatitude.ToString()), "CityLatitude");
                content.Add(new StringContent(model.CityLongitude.ToString()), "CityLongitude");

                if (model.AmenityIds != null)
                {
                    foreach (var amId in model.AmenityIds)
                        content.Add(new StringContent(amId.ToString()), "AmenityIds");
                }

                if (model.NewPhotos != null)
                {
                    foreach (var file in model.NewPhotos)
                    {
                        var fileContent = new StreamContent(file.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        content.Add(fileContent, "NewPhotos", file.FileName);
                    }
                }

                var response = await _client.PutAsync($"api/Listings/{id}", content);
                return response.IsSuccessStatusCode;
            }
        }

        // 7. Delete Listing (DELETE)
        public async Task<bool> DeleteListingAsync(int id)
        {
            AddAuthorization();
            var response = await _client.DeleteAsync($"api/Listings/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}