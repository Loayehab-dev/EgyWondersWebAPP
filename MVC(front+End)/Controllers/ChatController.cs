using Microsoft.AspNetCore.Mvc;
using MVC_front_End_.Services;
using System.Threading.Tasks;

namespace MVC_front_End_.Controllers
{
    public class ChatController : Controller
    {
        private readonly AIService _aiService;

        public ChatController(AIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<IActionResult> AskCleo([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message)) return BadRequest();

            var answer = await _aiService.GetRecommendationAsync(request.Message);
            return Ok(new { response = answer });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}