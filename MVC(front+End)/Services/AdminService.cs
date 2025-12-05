
using MVC_front_End_.Models;
using System.Net.Http.Headers;

namespace MVC_front_End_.Services
{
    public class AdminService : IAdminService
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
        }

        // --- Helper: Automatically attach JWT from the current Admin session ---
        private void SetAdminToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("JWT")?.Value;

            // DEBUG: Check if we actually have a token
            if (string.IsNullOrEmpty(token))
            {
                // If you see this error, it means your API Login didn't return a token!
                throw new Exception("STOP: The User is logged in, but the JWT Token is MISSING or EMPTY.");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // 1. Get All Listings
        public async Task<List<ListingDTO>> GetAllListingsAsync()
        {
            SetAdminToken(); // Ensure Token is attached

            // Call the ADMIN-ONLY endpoint
            var response = await _client.GetAsync("api/Listings/admin/all");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ListingDTO>>();
            }

            // ★ DEBUG: Crash here so we see the Real Error on screen ★
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"ADMIN API ERROR: {response.StatusCode} - {error}");
        }
        public async Task<List<HostDocumentDTO>> GetPendingDocumentsAsync()
        {
            SetAdminToken();

         
            var response = await _client.GetAsync("api/HostDocuments/pending");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<HostDocumentDTO>>();
            }
            else
            {
                // ADD THIS BLOCK TO SEE THE ERROR
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Failed! Status: {response.StatusCode}. Reason: {errorContent}");
            }
        }

        public async Task<bool> ApproveDocumentAsync(int documentId)
        {
            SetAdminToken();
            // FIX: Changed "HostDocument" to "HostDocuments"
            var response = await _client.PostAsync($"api/HostDocuments/approve/{documentId}", null);

            return response.IsSuccessStatusCode;
        }
        // Inside AdminService.cs

        public async Task<bool> RejectDocumentAsync(int documentId)
        {
            SetAdminToken();

           
            var response = await _client.DeleteAsync($"api/HostDocuments/{documentId}");

            return response.IsSuccessStatusCode;
        }
        // 1. GET SINGLE LISTING (For View Details)
        public async Task<ListingDTO> GetListingByIdAsync(int id)
        {
            SetAdminToken();
            var response = await _client.GetAsync($"api/Listings/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ListingDTO>();
            }
            return null;
        }

        // 2. DELETE LISTING
        public async Task<bool> DeleteListingAsync(int id)
        {
            SetAdminToken();
            var response = await _client.DeleteAsync($"api/Listings/{id}");
            return response.IsSuccessStatusCode;
        }

        // 3. UPDATE STATUS (Approve/Reject)
        // This assumes your API has a generic Update or Status endpoint. 
        // If not, we will try to just update the status field via a PUT.
        public async Task<bool> UpdateListingStatusAsync(int id, string status)
        {
            SetAdminToken();


            string url = $"api/Listings/{id}/status?status={status}";

            // We send 'null' as content because the data is in the URL string above
            var response = await _client.PutAsync(url, null);

            return response.IsSuccessStatusCode;
        }
        public async Task<DashboardStatsViewModel> GetDashboardStatsAsync()
        {
            try
            {
                SetAdminToken();

                // This MUST match your API Controller Route [Route("api/[controller]")]
                var response = await _client.GetAsync("api/Admin/stats");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DashboardStatsViewModel>();
                }
            }
            catch
            {
                // Ignore errors and return 0s so the dashboard still loads
            }

            return new DashboardStatsViewModel(); // Returns all zeros
        }
        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            try
            {
                SetAdminToken();
                // Call the API endpoint
                var response = await _client.GetAsync("api/Admin/users");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<UserDTO>>();
                }
            }
            catch
            {
                // Ignore errors to prevent crash
            }

            return new List<UserDTO>();
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            SetAdminToken();
            var response = await _client.DeleteAsync($"api/Admin/users/{id}");

            // DEBUG: If it fails, crash so we see the error message
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"DELETE FAILED: {response.StatusCode} - {error}");
            }

            return response.IsSuccessStatusCode;
        }
        public async Task<List<BookingAdminDTO>> GetAllBookingsAsync()
        {
            try
            {
                SetAdminToken();
                var response = await _client.GetAsync("api/Admin/bookings");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<BookingAdminDTO>>();
                }
            }
            catch { /* Safe fail */ }
            return new List<BookingAdminDTO>();
        }
        // --- ADD THESE METHODS FOR REVIEWS ---

        public async Task<List<ReviewAdminDTO>> GetAllReviewsAsync()
        {
            SetAdminToken();

            // VERIFY THIS URL MATCHES YOUR API CONTROLLER
            var response = await _client.GetAsync("api/Admin/reviews");

            if (!response.IsSuccessStatusCode)
            {
              
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"API ERROR: {response.StatusCode} - {content}");
            }

            return await response.Content.ReadFromJsonAsync<List<ReviewAdminDTO>>();
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            SetAdminToken();
            var response = await _client.DeleteAsync($"api/Admin/reviews/{id}");
            return response.IsSuccessStatusCode;
        }

    }
}