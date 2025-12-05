using MVC_front_End_.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
namespace MVC_front_End_.Services
{
    public class AuthService : IAuthService

    {
        private readonly HttpClient _client;

        private readonly IHttpContextAccessor _httpContextAccessor;

        // Inject IHttpContextAccessor to get the logged-in user's Token
        public AuthService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
        }
        // --- Helper to add JWT to headers ---
        private void SetToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // 1. Login & Register
        public async Task<AuthResponseDto> LoginAsync(LoginDTO model)
        {
            var response = await _client.PostAsJsonAsync("api/Auth/login", model);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return null;
        }

        // Ensure this is at the top

        public async Task<AuthResponseDto> RegisterAsync(RegisterDTO model)
        {
            var response = await _client.PostAsJsonAsync("api/Auth/register", model);

            // --- 1. SUCCESS HANDLING ---
            if (response.IsSuccessStatusCode)
            {
                // Ideally, the API returns the DTO. But if it returns null/empty, 
                // we manually construct a success response.
                var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>()
                             ?? new AuthResponseDto();

                // FIX: Ensure this is TRUE so the UI knows it worked.
                // Even if the API didn't send a token (because email needs confirmation),
                // the *operation* was successful.
                result.IsAuthenticated = true;
                result.Message = "Registration successful. Please confirm your email before login.";

                return result;
            }

            // --- 2. ERROR HANDLING (Your existing robust logic) ---
            var errorContent = await response.Content.ReadAsStringAsync();
            string finalErrorMessage = "Registration failed.";

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(errorContent))
                {
                    JsonElement root = doc.RootElement;
                    // (Same error parsing logic as before...)
                    if (root.TryGetProperty("errors", out JsonElement errorsElement))
                    {
                        var firstError = errorsElement.EnumerateObject().FirstOrDefault();
                        finalErrorMessage = firstError.Value.ValueKind == JsonValueKind.Array
                            ? firstError.Value[0].GetString()
                            : firstError.Name;
                    }
                    else if (root.TryGetProperty("message", out JsonElement msgElement))
                    {
                        finalErrorMessage = msgElement.GetString();
                    }
                    else if (root.TryGetProperty("error", out JsonElement errorElement))
                    {
                        finalErrorMessage = errorElement.GetString();
                    }
                }
            }
            catch
            {
                finalErrorMessage = !string.IsNullOrWhiteSpace(errorContent)
                    ? errorContent
                    : response.ReasonPhrase;
            }

            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = finalErrorMessage
            };
        }
        // 2. Profile Management
        public async Task<UserDTO> GetProfileAsync(string token)
        {
            SetToken(token);
            return await _client.GetFromJsonAsync<UserDTO>("api/Auth/profile");
        }

        public async Task<bool> UpdateProfileAsync(UpdateProfileDTO model, string token)
        {
            SetToken(token);
            var response = await _client.PutAsJsonAsync("api/Auth/profile", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<string> ChangePasswordAsync(ChangePasswordDTO model, string token)
        {
            // 1. Attach the Token (Critical for knowing WHO is changing the password)
            SetToken(token);

            // 2. Send the request
            var response = await _client.PostAsJsonAsync("api/Auth/change-password", model);

            // 3. Return "Success" or the Error Message
            if (response.IsSuccessStatusCode)
            {
                return "Success";
            }

            // Read the error message from the API (e.g. "Incorrect current password")
            var errorContent = await response.Content.ReadAsStringAsync();

            // Return the error, or a default message if empty
            return !string.IsNullOrEmpty(errorContent) ? errorContent : "Failed to change password";
        }

        public async Task<bool> DeleteAccountAsync(DeleteAccountDTO model, string token)
        {
            SetToken(token);
            // Delete usually doesn't take a body in standard HTTP, but your API does.
            // We use SendAsync to send a body with DELETE.
            var request = new HttpRequestMessage(HttpMethod.Delete, "api/Auth/account")
            {
                Content = JsonContent.Create(model)
            };
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // 3. Admin Actions (Consuming the endpoints you provided)
        public async Task<List<UserDTO>> GetAllUsersAsync(string token)
        {
            SetToken(token);
            return await _client.GetFromJsonAsync<List<UserDTO>>("api/Auth/users");
        }

        public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive, string token)
        {
            SetToken(token);
            var response = await _client.PatchAsync($"api/Auth/users/{userId}/status?isActive={isActive}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AssignRoleAsync(AssignRoleDTO model, string token)
        {
            SetToken(token);
            var response = await _client.PostAsJsonAsync("api/Auth/assign-role", model);
            return response.IsSuccessStatusCode;
        }
        public async Task<string> ResetPasswordAsync(ResetPasswordDTO model)
        {
            var response = await _client.PostAsJsonAsync("api/Auth/reset-password", model);
            // Note: The controller checks specifically for "Password Reset"
            return response.IsSuccessStatusCode ? "Password Reset" : "Error";
        }
        public async Task<string> ForgotPasswordAsync(string email)
        {
            // 1. Create the DTO expected by the API
            var model = new ForgotPasswordDTO { Email = email };

            // 2. Call the endpoint
            var response = await _client.PostAsJsonAsync("api/Auth/forgot-password", model);

            // 3. Return status
            return response.IsSuccessStatusCode ? "Success" : "Error";
        }
        public async Task<AuthResponseDto> GoogleLoginAsync(string idToken)
        {
            // Create the DTO expected by your API
            var model = new GoogleLoginDTO { IdToken = idToken };

            var response = await _client.PostAsJsonAsync("api/Auth/google-login", model);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            }
            return null;
        }
        public async Task<string> ConfirmEmailAsync(string userId, string token)
        {
            // The API expects parameters in the Query String
            var response = await _client.GetAsync($"api/Auth/confirm-email?userId={userId}&token={WebUtility.UrlEncode(token)}");

            if (response.IsSuccessStatusCode)
            {
                return "Success";
            }

            return "Failed to confirm email. Link may be expired.";
        }


        public async Task<bool> RegisterHostAsync(HostDocumentCreateDTO model)
        {
            using (var content = new MultipartFormDataContent())
            {
                // 1. UserId (integer($int32))
                // We send it as a string, the API will convert it to int automatically
                content.Add(new StringContent(model.UserId.ToString()), "UserId");

                // 2. NationalId (string)
                content.Add(new StringContent(model.NationalId ?? ""), "NationalId");

                // 3. TextRecord (string - Optional)
                if (!string.IsNullOrEmpty(model.TextRecord))
                {
                    content.Add(new StringContent(model.TextRecord), "TextRecord");
                }

                // 4. Document (string($binary))
                if (model.Document != null)
                {
                    var fileStream = model.Document.OpenReadStream();
                    var fileContent = new StreamContent(fileStream);

                    // Content-Type is important (e.g. "image/jpeg")
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.Document.ContentType);

                    // "Document" must match the API parameter name exactly
                    content.Add(fileContent, "Document", model.Document.FileName);
                }

                // 5. ATTACH TOKEN (Essential for [Authorize] API endpoints)
                var token = _httpContextAccessor.HttpContext?.User.FindFirst("JWT")?.Value
                            ?? _httpContextAccessor.HttpContext?.Request.Cookies["JWT"]; // Or however you store it

                if (!string.IsNullOrEmpty(token))
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // 6. Send Request
                var response = await _client.PostAsync("api/HostDocuments", content);

                // --- DEBUGGING: THROW THE ACTUAL ERROR ---
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    // This will crash the app intentionally so you can see the error
                    throw new Exception($"API Error ({response.StatusCode}): {errorContent}");
                }
                // ----------------------------------------

                return true;
            }
        }
        public async Task LogoutAsync()
        {
            // If your HttpClient is re-used, clear the header so the next request 
            // doesn't accidentally use the old token.
            _client.DefaultRequestHeaders.Authorization = null;

            // If you are storing the JWT in Session or LocalStorage, clear it here.
            // Since we are using Cookies in MVC, the Controller handles the main logout.
            await Task.CompletedTask;
        }
        public async Task<UserProfileDTO> GetUserProfileAsync(string token)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Make sure this URL matches your actual API endpoint
            var response = await _client.GetAsync("api/Auth/profile");

            if (response.IsSuccessStatusCode)
            {
                // This automatically maps JSON fields to your DTO properties
                return await response.Content.ReadFromJsonAsync<UserProfileDTO>();
            }
            return null;
        }
        public async Task<bool> UpdateUserProfileAsync(UpdateProfileDTO model, string token)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // API endpoint: PUT request to update the profile resource
            var response = await _client.PutAsJsonAsync("api/Auth/profile", model);

            return response.IsSuccessStatusCode;
        }
    }


    }