using AIChatWebApp.Models;
using AIChatWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIChatWebApp.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly IGroqService _ai;

        public ChatController(IGroqService ai)
        {
            _ai = ai;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Message))
                return BadRequest("Message is required");

            var reply = await _ai.AskAI(req.Message);

            return Ok(reply);
        }
    }
}
