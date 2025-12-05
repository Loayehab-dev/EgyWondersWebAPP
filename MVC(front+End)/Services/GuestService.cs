using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json; 
using System.Threading.Tasks;
using MVC_front_End_.Models;

namespace MVC_front_End_.Services
{
    public class GuestService : IGuestService
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;

        public GuestService(HttpClient client)
        {
            _client = client;
           
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<ListingDTO>> GetListingsAsync(string search = null, string category = null, decimal? minPrice = null, decimal? maxPrice = null, int? capacity = null)
        {
            try
            {
                // 1. Fetch ALL listings from API
                var response = await _client.GetAsync("api/Listings");

                if (response.IsSuccessStatusCode)
                {
                    // Use the options here
                    var listings = await response.Content.ReadFromJsonAsync<List<ListingDTO>>(_options);

                    if (listings == null) return new List<ListingDTO>();

                    var query = listings.AsQueryable();

                    // 2. Filter Logic
                    if (!string.IsNullOrEmpty(search))
                    {
                        search = search.ToLower();
                        query = query.Where(l =>
                            (l.Title != null && l.Title.ToLower().Contains(search)) ||
                            (l.CityName != null && l.CityName.ToLower().Contains(search))
                        );
                    }

                    if (!string.IsNullOrEmpty(category))
                        query = query.Where(l => l.Category == category);

                    if (minPrice.HasValue)
                        query = query.Where(l => l.PricePerNight >= minPrice.Value);

                    if (maxPrice.HasValue)
                        query = query.Where(l => l.PricePerNight <= maxPrice.Value);

                    if (capacity.HasValue)
                        query = query.Where(l => l.Capacity >= capacity.Value);

                    // ★ FIX 2: Commented out status check for debugging.
                    // If your DB has "Pending" listings, they will now show up.
                    // query = query.Where(l => l.Status == "Active");

                    return query.ToList();
                }
            }
            catch
            {
                // If API fails, return empty list instead of crashing
                return new List<ListingDTO>();
            }

            return new List<ListingDTO>();
        }

        public async Task<ListingDTO> GetListingByIdAsync(int id)
        {
            try
            {
                var response = await _client.GetAsync($"api/Listings/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ListingDTO>(_options);
                }
            }
            catch { }
            return null;
        }
    }
}