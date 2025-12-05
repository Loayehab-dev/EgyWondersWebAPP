using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MVC_front_End_.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        // Your API Key
        private readonly string _apiKey = "AIzaSyD3Yl0o3_KbMxcvXLMMnOEOXYWNy7JLrFo";

        public AIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetRecommendationAsync(string userMessage)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "API Key is missing.";

            try
            {
                // 1. Prepare the prompt
                var requestData = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text =
                            $"You are Cloe, a friendly Egypt tour guide. Give a  recommendation and act like real tour guide for: {userMessage}"
                        }}}
                    }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                // ★ FIX: Use 'gemini-2.5-flash' (The current active model in late 2025)
                string model = "gemini-2.5-flash";
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

                var response = await _httpClient.PostAsync(url, jsonContent);

                // 2. Fallback: If 2.5 fails, try 2.0-flash-exp
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    model = "gemini-2.0-flash-exp";
                    url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";
                    response = await _httpClient.PostAsync(url, jsonContent);
                }

                // 3. Success Handling
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        return candidates[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();
                    }
                }

                // 4. Debugging: Log the error if it still fails
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Gemini Error ({model}): {response.StatusCode} - {error}");

                return GetOfflineRecommendation(userMessage);
            }
            catch
            {
                return GetOfflineRecommendation(userMessage);
            }
        }

        // Hardcoded responses (Fail-Safe)
        private string GetOfflineRecommendation(string input)
        {
            input = input.ToLower();

            if (input.Contains("adventure") || input.Contains("sea") || input.Contains("dive"))
                return "Since you like adventure, I highly recommend **Dahab** (The Blue Hole) or **Sharm El Sheikh** for diving! 🌊";

            if (input.Contains("history") || input.Contains("old") || input.Contains("culture") || input.Contains("temple"))
                return "For history lovers, you simply must visit **Luxor** (The Valley of the Kings) and the temples of **Aswan**! 🏛️";

            if (input.Contains("relax") || input.Contains("quiet") || input.Contains("chill"))
                return "For a relaxing trip, try a **Nile Cruise** from Aswan or visit the beautiful **Siwa Oasis**. 🌴";

            return "Welcome to Egypt! I recommend starting with the **Pyramids of Giza** and the **Egyptian Museum** in Cairo. 🇪🇬";
        }
    }
}