using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using MVC_front_End_.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace MVC_front_End_.Services
{
    public class GuestBookingService
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContext;

        public GuestBookingService(HttpClient client, IHttpContextAccessor httpContext)
        {
            _client = client;
            _httpContext = httpContext;
        }

        private void AddAuthorization()
        {
            var token = _httpContext.HttpContext?.User.FindFirst("JWT")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // 1. Create Booking (Pending)
        public async Task<BookingDTO> CreateBookingAsync(BookingCreateDTO model)
        {
            AddAuthorization();
            var response = await _client.PostAsJsonAsync("api/ListingBooking", model);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<BookingDTO>();
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Booking Failed: {error}");
        }

        // 2. Record Payment (Confirmed)
        public async Task<string> RecordPaymentAsync(PaymentCreateDto model)
        {
            var token = _httpContext.HttpContext?.User.FindFirst("JWT")?.Value;
            if (!string.IsNullOrEmpty(token)) _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsJsonAsync("api/Payments/listing", model);

            if (response.IsSuccessStatusCode)
            {
                return "Success";
            }

            // Return the real error from API so we can see it in the red box
            var error = await response.Content.ReadAsStringAsync();
            return string.IsNullOrEmpty(error) ? $"Error Code: {response.StatusCode}" : error;
        }

        // ★ NEW: Get User Bookings (For "My Trips" Page) ★
        public async Task<List<BookingDTO>> GetUserBookingsAsync(int userId)
        {
            AddAuthorization();
            try
            {
                // Calls: GET api/ListingBooking/user/5
                var bookings = await _client.GetFromJsonAsync<List<BookingDTO>>($"api/ListingBooking/user/{userId}");
                return bookings ?? new List<BookingDTO>();
            }
            catch
            {
                return new List<BookingDTO>();
            }
        }
        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var token = _httpContext.HttpContext?.User.FindFirst("JWT")?.Value;
            if (!string.IsNullOrEmpty(token))
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Call the API Endpoint: DELETE api/ListingBooking/{id}
            var response = await _client.DeleteAsync($"api/ListingBooking/{bookingId}");

            return response.IsSuccessStatusCode;
        }
        public async Task<bool> AddReviewAsync(ReviewCreateDTO model)
        {
            AddAuthorization(); // Ensures token is sent
            var response = await _client.PostAsJsonAsync("api/Reviews", model);
            return response.IsSuccessStatusCode;
        }

    }
}